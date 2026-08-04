using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class MemberQueriesHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task GetMembers_ShouldSortByNameAndExcludeSoftDeleted()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldSortByNameAndExcludeSoftDeleted));

        dbContext.Members.AddRange(
            ActiveMember(faker.Name.FirstName(), "Brown"),
            ExpiredMember(faker.Name.FirstName(), "Anderson"),
            new Member(faker.Name.FirstName(), "Zimmer") { IsActive = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);

        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        result.Items.Count.ShouldBe(2);
        result.Items[0].LastName.ShouldBe("Anderson");
        result.Items[1].LastName.ShouldBe("Brown");
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetMembers_ShouldDeriveTheThreeStandingsFromTheSubscription()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldDeriveTheThreeStandingsFromTheSubscription));

        dbContext.Members.AddRange(
            ActiveMember(faker.Name.FirstName(), "Active"),
            ExpiringSoonMember(faker.Name.FirstName(), "Expiring"),
            ExpiredMember(faker.Name.FirstName(), "Expired"),
            new Member(faker.Name.FirstName(), "NoSubscription"));
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        StandingOf(result, "Active").ShouldBe(MemberStatus.Active);
        StandingOf(result, "Expiring").ShouldBe(MemberStatus.ExpiringSoon);
        StandingOf(result, "Expired").ShouldBe(MemberStatus.Inactive);
        StandingOf(result, "NoSubscription").ShouldBe(MemberStatus.Inactive);

        // The chip counts partition the list exactly.
        result.TotalCount.ShouldBe(4);
        result.ActiveCount.ShouldBe(1);
        result.ExpiringSoonCount.ShouldBe(1);
        result.InactiveCount.ShouldBe(2);
        (result.ActiveCount + result.ExpiringSoonCount + result.InactiveCount).ShouldBe(result.TotalCount);
    }

    [Fact]
    public async Task GetMembers_ShouldTreatTheLongestCoverAsTheOneThatCounts()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldTreatTheLongestCoverAsTheOneThatCounts));

        // One cover ends tomorrow, another runs for months: the member is active,
        // not "expiring soon".
        var member = new Member(faker.Name.FirstName(), "Overlap")
        {
            Subscriptions =
            [
                new Subscription { StartDate = Today.AddDays(-30), EndDate = Today.AddDays(1) },
                new Subscription { StartDate = Today.AddDays(-2), EndDate = Today.AddMonths(6) }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        StandingOf(result, "Overlap").ShouldBe(MemberStatus.Active);
    }

    [Fact]
    public async Task GetMembers_ShouldIgnoreSoftDeletedSubscriptions()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldIgnoreSoftDeletedSubscriptions));

        var member = new Member(faker.Name.FirstName(), "Cancelled")
        {
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = Today.AddDays(-10),
                    EndDate = Today.AddMonths(3),
                    IsActive = false
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        StandingOf(result, "Cancelled").ShouldBe(MemberStatus.Inactive);
    }

    [Theory]
    [InlineData(MemberStatus.Active, "Active")]
    [InlineData(MemberStatus.ExpiringSoon, "Expiring")]
    [InlineData(MemberStatus.Inactive, "Expired")]
    public async Task GetMembers_ShouldFilterByStanding(MemberStatus status, string expectedLastName)
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(
            $"{nameof(GetMembers_ShouldFilterByStanding)}-{status}");

        dbContext.Members.AddRange(
            ActiveMember(faker.Name.FirstName(), "Active"),
            ExpiringSoonMember(faker.Name.FirstName(), "Expiring"),
            ExpiredMember(faker.Name.FirstName(), "Expired"));
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var result = await handler.Handle(new GetMembersQuery { Status = status }, CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].LastName.ShouldBe(expectedLastName);
        result.FilteredCount.ShouldBe(1);

        // The chip counts stay on the whole search, not on the filtered slice.
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetMembers_ShouldSearchNameEmailAndPhone()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldSearchNameEmailAndPhone));

        dbContext.Members.AddRange(
            new Member("Laetitia", "Moriceau") { Email = "laetitia.moriceau@gymxyz.fr", Phone = "06 12 34 56 78" },
            new Member("Lucas", "Martin") { Email = "lucas.martin@gymxyz.fr", Phone = "06 80 45 12 33" });
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);

        (await handler.Handle(new GetMembersQuery { Search = "Moriceau" }, CancellationToken.None))
            .Items.Single().FirstName.ShouldBe("Laetitia");

        (await handler.Handle(new GetMembersQuery { Search = "lucas.martin@" }, CancellationToken.None))
            .Items.Single().FirstName.ShouldBe("Lucas");

        (await handler.Handle(new GetMembersQuery { Search = "80 45" }, CancellationToken.None))
            .Items.Single().FirstName.ShouldBe("Lucas");

        // The counts follow the search, so a chip never promises a row the list
        // cannot show.
        var searched = await handler.Handle(new GetMembersQuery { Search = "Moriceau" }, CancellationToken.None);
        searched.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetMembers_ShouldPageServerSide()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldPageServerSide));

        for (var index = 0; index < 7; index++)
        {
            dbContext.Members.Add(new Member("Membre", $"N{index:D2}"));
        }

        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);

        var firstPage = await handler.Handle(new GetMembersQuery { Page = 1, PageSize = 3 }, CancellationToken.None);
        firstPage.Items.Count.ShouldBe(3);
        firstPage.Items[0].LastName.ShouldBe("N00");
        firstPage.FilteredCount.ShouldBe(7);

        var lastPage = await handler.Handle(new GetMembersQuery { Page = 3, PageSize = 3 }, CancellationToken.None);
        lastPage.Items.Count.ShouldBe(1);
        lastPage.Items[0].LastName.ShouldBe("N06");
    }

    [Fact]
    public async Task GetMembers_ShouldLeaveTheColumnsOfLaterLotsUnset()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldLeaveTheColumnsOfLaterLotsUnset));

        dbContext.Members.Add(ActiveMember("Laetitia", "Moriceau"));
        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var member = (await handler.Handle(new GetMembersQuery(), CancellationToken.None)).Items.Single();

        member.PlanLabel.ShouldBeNull();
        member.CreditsLabel.ShouldBeNull();
        member.CreditsPercent.ShouldBeNull();
        member.AttendanceRate.ShouldBeNull();
        member.LastVisitOn.ShouldBeNull();
    }

    [Fact]
    public async Task GetMembers_ShouldOnlyReturnTheAmbientTenant()
    {
        var tenantContext = new TestTenantContext(TestInfrastructure.DefaultTenantId);
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetMembers_ShouldOnlyReturnTheAmbientTenant), tenantContext);

        dbContext.Members.Add(new Member("Laetitia", "Moriceau"));
        await dbContext.SaveChangesAsync();

        using (tenantContext.UseTenant(TestInfrastructure.DefaultTenantId + 1))
        {
            dbContext.Members.Add(new Member("Autre", "Client"));
            await dbContext.SaveChangesAsync();
        }

        var handler = new GetMembersQueryHandler(dbContext);
        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].LastName.ShouldBe("Moriceau");
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetMemberById_ShouldReturnNull_WhenNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMemberById_ShouldReturnNull_WhenNotFound));
        var handler = new GetMemberByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetMemberByIdQuery(12345), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetMemberById_ShouldReturnMemberDto_WhenFound()
    {
        var faker = TestInfrastructure.Faker();
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var email = faker.Internet.Email();
        var street = faker.Address.StreetAddress();
        var zipCode = faker.Address.ZipCode();
        var city = faker.Address.City();
        var country = faker.Address.Country();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMemberById_ShouldReturnMemberDto_WhenFound));

        var member = new Member(firstName, lastName)
        {
            Email = email,
            Address = new Address
            {
                Street = street,
                ZipCode = zipCode,
                City = city,
                Country = country
            }
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetMemberByIdQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(member.Id);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
        result.Email.ShouldBe(email);
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe(street);
    }

    private static MemberStatus StandingOf(MembersPageDto page, string lastName)
        => page.Items.Single(item => item.LastName == lastName).Status;

    /// <summary>Cover running well past the warning window.</summary>
    private static Member ActiveMember(string firstName, string lastName) =>
        new(firstName, lastName)
        {
            Subscriptions = [new Subscription { StartDate = Today.AddDays(-10), EndDate = Today.AddMonths(3) }]
        };

    /// <summary>Cover ending inside the warning window.</summary>
    private static Member ExpiringSoonMember(string firstName, string lastName) =>
        new(firstName, lastName)
        {
            Subscriptions = [new Subscription { StartDate = Today.AddDays(-20), EndDate = Today.AddDays(3) }]
        };

    private static Member ExpiredMember(string firstName, string lastName) =>
        new(firstName, lastName)
        {
            Subscriptions = [new Subscription { StartDate = Today.AddMonths(-3), EndDate = Today.AddMonths(-1) }]
        };
}
