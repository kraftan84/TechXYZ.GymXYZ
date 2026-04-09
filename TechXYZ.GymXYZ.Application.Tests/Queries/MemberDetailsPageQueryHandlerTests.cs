using Shouldly;
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
            Room = new Room("Room A"),
            Member = member,
            StartDate = DateTime.Today.AddDays(-2).AddHours(10),
            EndDate = DateTime.Today.AddDays(-2).AddHours(11)
        };
        var collectiveLesson = new CollectiveLesson
        {
            Name = "Collective",
            Type = LessonType.Collective,
            Coach = coach,
            Rooms = [new Room("Room B")],
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
        result.Lessons.Count.ShouldBe(2);
        result.Stats.TotalLessons.ShouldBe(2);
        result.Stats.LastVisit.ShouldNotBeNull();
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
