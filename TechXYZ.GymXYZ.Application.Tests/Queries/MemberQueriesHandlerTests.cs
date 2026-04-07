using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class MemberQueriesHandlerTests
{
    [Fact]
    public async Task GetMembers_ShouldReturnSortedListWithActiveSubscriptionFlag()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMembers_ShouldReturnSortedListWithActiveSubscriptionFlag));

        var activeMember = new Member(faker.Name.FirstName(), "Brown")
        {
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
                }
            ]
        };

        var inactiveMember = new Member(faker.Name.FirstName(), "Anderson")
        {
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1))
                }
            ]
        };

        dbContext.Members.AddRange(activeMember, inactiveMember);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new GetMembersQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result[0].LastName.ShouldBe("Anderson");
        result[0].HasActiveSubscription.ShouldBeFalse();
        result[1].LastName.ShouldBe("Brown");
        result[1].HasActiveSubscription.ShouldBeTrue();
    }

    [Fact]
    public async Task GetMemberById_ShouldReturnNull_WhenNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetMemberById_ShouldReturnNull_WhenNotFound));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new GetMemberByIdQueryHandler(unitOfWork);

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

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new GetMemberByIdQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetMemberByIdQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(member.Id);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
        result.Email.ShouldBe(email);
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe(street);
    }
}
