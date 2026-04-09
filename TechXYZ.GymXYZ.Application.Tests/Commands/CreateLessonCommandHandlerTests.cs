using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateLessonCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCollectiveLesson()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateCollectiveLesson));
        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        var room = new Room(faker.Commerce.ProductName());
        var theme = new LessonTheme(faker.Commerce.Department());
        dbContext.Coaches.Add(coach);
        dbContext.Rooms.Add(room);
        dbContext.LessonThemes.Add(theme);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLessonCommandHandler(dbContext, new CreateLessonCommandValidator());
        var startDate = DateTime.UtcNow.Date.AddHours(9);
        var endDate = startDate.AddHours(1);

        var lessonId = await handler.Handle(
            new CreateLessonCommand(
                $" {faker.Commerce.ProductName()} ",
                faker.Lorem.Sentence(),
                LessonType.Collective,
                theme.Id,
                coach.Id,
                startDate,
                endDate,
                room.Id,
                15),
            CancellationToken.None);

        var lesson = await dbContext.CollectiveLessons.FindAsync(lessonId);
        lesson.ShouldNotBeNull();
        lesson!.Name.ShouldNotBeNullOrWhiteSpace();
        lesson.MaxParticipants.ShouldBe(15);
    }

    [Fact]
    public async Task Handle_ShouldCreatePrivateLesson()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreatePrivateLesson));
        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        var room = new Room(faker.Commerce.ProductName());
        dbContext.Coaches.Add(coach);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLessonCommandHandler(dbContext, new CreateLessonCommandValidator());
        var startDate = DateTime.UtcNow.Date.AddHours(11);
        var endDate = startDate.AddHours(1);

        var lessonId = await handler.Handle(
            new CreateLessonCommand(
                faker.Commerce.ProductName(),
                null,
                LessonType.Private,
                null,
                coach.Id,
                startDate,
                endDate,
                room.Id,
                null),
            CancellationToken.None);

        dbContext.PrivateLessons.Any(lesson => lesson.Id == lessonId).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCoachDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenCoachDoesNotExist));
        dbContext.Rooms.Add(new Room("Room"));
        await dbContext.SaveChangesAsync();

        var handler = new CreateLessonCommandHandler(dbContext, new CreateLessonCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateLessonCommand(
                "Lesson",
                null,
                LessonType.Private,
                null,
                999,
                DateTime.UtcNow.Date.AddHours(9),
                DateTime.UtcNow.Date.AddHours(10),
                1,
                null),
            CancellationToken.None));
    }
}
