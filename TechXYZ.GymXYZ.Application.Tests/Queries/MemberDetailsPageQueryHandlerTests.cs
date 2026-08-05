using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class MemberDetailsPageQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMemberDetailsPage()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnMemberDetailsPage));

        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Email = faker.Internet.Email(),
            Phone = faker.Phone.PhoneNumber(),
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };

        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        var pastSession = new Session
        {
            CourseTemplate = new CourseTemplate("Private") { Discipline = new Discipline("Coaching") },
            Coach = coach,
            Location = new Location("Location A"),
            Capacity = 1,
            StartsAt = DateTime.Today.AddDays(-2).AddHours(10),
            EndsAt = DateTime.Today.AddDays(-2).AddHours(11)
        };
        var upcomingSession = new Session
        {
            CourseTemplate = new CourseTemplate("Collective") { Discipline = new Discipline("Renforcement") },
            Coach = coach,
            Location = new Location("Location B"),
            Capacity = 20,
            StartsAt = DateTime.Today.AddDays(2).AddHours(18),
            EndsAt = DateTime.Today.AddDays(2).AddHours(19)
        };
        var subscription = new Subscription
        {
            Member = member,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            NumberOfSessions = 12
        };

        member.Registrations =
        [
            new Registration { Session = pastSession, RegisteredAt = DateTime.Today.AddDays(-9) },
            new Registration { Session = upcomingSession, RegisteredAt = DateTime.Today.AddDays(-1) }
        ];
        member.Subscriptions = [subscription];

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(member.Id);
        result.Subscriptions.Count.ShouldBe(1);
        // "depuis l'inscription" counts what the member has been to, not the
        // seats already booked for the weeks ahead.
        result.Stats.TotalSessions.ShouldBe(1);

        // Sessions are split by the record's two cards.
        result.UpcomingSessions.Count.ShouldBe(1);
        result.UpcomingSessions[0].Name.ShouldBe("Collective");
        result.PastSessions.Count.ShouldBe(1);
        result.PastSessions[0].Name.ShouldBe("Private");

        // A capacity of one is what makes a session private — there is no type.
        result.PastSessions[0].IsPrivate.ShouldBeTrue();
        result.UpcomingSessions[0].IsPrivate.ShouldBeFalse();
        result.UpcomingSessions[0].RemainingSpots.ShouldBe(19);

        // Nothing on this member's seats has been pointed, so there is no
        // assiduité to show — null, not nought, and the card reads "—".
        result.Stats.AttendanceRate.ShouldBeNull();
        result.Stats.LastVisitOn.ShouldBeNull();

        // Payments arrive with the subscriptions section (lot 7).
        result.Payments.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCarryJoinDateNotesAndDerivedStanding()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCarryJoinDateNotesAndDerivedStanding));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            JoinedOn = today.AddMonths(-14),
            BirthDate = today.AddYears(-32),
            Notes = "Préfère les cours du matin.",
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = today.AddDays(-20),
                    EndDate = today.AddDays(3),
                    NumberOfSessions = 10
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.JoinedOn.ShouldBe(today.AddMonths(-14));
        result.BirthDate.ShouldBe(today.AddYears(-32));
        result.Notes.ShouldBe("Préfère les cours du matin.");
        result.Status.ShouldBe(MemberStatus.ExpiringSoon);
        result.CurrentSubscription.ShouldNotBeNull();
        result.CurrentSubscription!.EndDate.ShouldBe(today.AddDays(3));
    }

    [Fact]
    public async Task Handle_ShouldReportInactive_WhenNoSubscriptionCoversToday()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReportInactive_WhenNoSubscriptionCoversToday));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = today.AddMonths(-4),
                    EndDate = today.AddMonths(-1)
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Status.ShouldBe(MemberStatus.Inactive);
        result.CurrentSubscription.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMemberNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnNull_WhenMemberNotFound));
        var handler = new GetMemberDetailsPageQueryHandler(dbContext);

        var result = await handler.Handle(new GetMemberDetailsPageQuery(12345), CancellationToken.None);

        result.ShouldBeNull();
    }
}
