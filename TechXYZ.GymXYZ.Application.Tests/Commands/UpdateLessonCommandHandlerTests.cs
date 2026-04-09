using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateLessonCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdatePrivateLesson()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdatePrivateLesson));
        var coach = new Coach("John", "Doe");
        var coach2 = new Coach("Jane", "Doe");
        var room = new Room("Studio A");
        var room2 = new Room("Studio B");
        var lesson = new PrivateLesson
        {
            Name = "Lesson",
            Type = LessonType.Private,
            Coach = coach,
            Room = room,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.Coaches.AddRange(coach, coach2);
        dbContext.Rooms.AddRange(room, room2);
        dbContext.PrivateLessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLessonCommandHandler(dbContext, new UpdateLessonCommandValidator());
        var result = await handler.Handle(
            new UpdateLessonCommand(
                lesson.Id,
                "Updated lesson",
                "desc",
                LessonType.Private,
                null,
                coach2.Id,
                DateTime.UtcNow.Date.AddHours(12),
                DateTime.UtcNow.Date.AddHours(13),
                room2.Id,
                null),
            CancellationToken.None);

        result.ShouldBeTrue();
        lesson.Name.ShouldBe("Updated lesson");
        lesson.Coach.Id.ShouldBe(coach2.Id);
        lesson.Room.Id.ShouldBe(room2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenLessonNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenLessonNotFound));
        var coach = new Coach("John", "Doe");
        var room = new Room("Studio A");
        dbContext.Coaches.Add(coach);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLessonCommandHandler(dbContext, new UpdateLessonCommandValidator());
        var result = await handler.Handle(
            new UpdateLessonCommand(
                999,
                "Updated lesson",
                null,
                LessonType.Private,
                null,
                coach.Id,
                DateTime.UtcNow.Date.AddHours(12),
                DateTime.UtcNow.Date.AddHours(13),
                room.Id,
                null),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenTypeChanges()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenTypeChanges));
        var coach = new Coach("John", "Doe");
        var room = new Room("Studio A");
        var lesson = new PrivateLesson
        {
            Name = "Lesson",
            Type = LessonType.Private,
            Coach = coach,
            Room = room,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.Coaches.Add(coach);
        dbContext.Rooms.Add(room);
        dbContext.PrivateLessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLessonCommandHandler(dbContext, new UpdateLessonCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new UpdateLessonCommand(
                lesson.Id,
                "Lesson",
                null,
                LessonType.Collective,
                null,
                coach.Id,
                DateTime.UtcNow.Date.AddHours(10),
                DateTime.UtcNow.Date.AddHours(11),
                room.Id,
                10),
            CancellationToken.None));
    }
}
