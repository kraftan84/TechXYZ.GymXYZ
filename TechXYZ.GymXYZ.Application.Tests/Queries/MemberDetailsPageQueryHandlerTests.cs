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
        var privateLesson = new PrivateLesson
        {
            Name = "Private",
            Type = LessonType.Private,
            Coach = coach,
            Location = new Location("Location A"),
            Member = member,
            StartDate = DateTime.Today.AddDays(-2).AddHours(10),
            EndDate = DateTime.Today.AddDays(-2).AddHours(11)
        };
        var collectiveLesson = new CollectiveLesson
        {
            Name = "Collective",
            Type = LessonType.Collective,
            Coach = coach,
            Locations = [new Location("Location B")],
            Participants = [member],
            MaxParticipants = 20,
            StartDate = DateTime.Today.AddDays(2).AddHours(18),
            EndDate = DateTime.Today.AddDays(2).AddHours(19)
        };
        var subscription = new Subscription
        {
            Member = member,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            NumberOfLessons = 12
        };

        member.PrivateLessons = [privateLesson];
        member.CollectiveLessons = [collectiveLesson];
        member.Subscriptions = [subscription];

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(member.Id);
        result.Subscriptions.Count.ShouldBe(1);
        result.Stats.TotalLessons.ShouldBe(2);

        // Sessions are split by the record's two cards.
        result.UpcomingLessons.Count.ShouldBe(1);
        result.UpcomingLessons[0].Name.ShouldBe("Collective");
        result.PastLessons.Count.ShouldBe(1);
        result.PastLessons[0].Name.ShouldBe("Private");

        // Attendance is produced by check-in (lot 6) — never approximated here.
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
                    NumberOfLessons = 10
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
