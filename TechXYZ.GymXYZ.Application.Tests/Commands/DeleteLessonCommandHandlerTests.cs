using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteLessonCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteLesson()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteLesson));
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

        var handler = new DeleteLessonCommandHandler(dbContext, new DeleteLessonCommandValidator());
        var result = await handler.Handle(new DeleteLessonCommand(lesson.Id), CancellationToken.None);

        result.ShouldBeTrue();
        lesson.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenLessonNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenLessonNotFound));
        var handler = new DeleteLessonCommandHandler(dbContext, new DeleteLessonCommandValidator());

        var result = await handler.Handle(new DeleteLessonCommand(999), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
