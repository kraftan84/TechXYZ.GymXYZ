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
        var location = new Location("Studio A");
        var location2 = new Location("Studio B");
        var lesson = new PrivateLesson
        {
            Name = "Lesson",
            Type = LessonType.Private,
            Coach = coach,
            Location = location,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.Coaches.AddRange(coach, coach2);
        dbContext.Locations.AddRange(location, location2);
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
                location2.Id,
                null),
            CancellationToken.None);

        result.ShouldBeTrue();
        lesson.Name.ShouldBe("Updated lesson");
        lesson.Coach.Id.ShouldBe(coach2.Id);
        lesson.Location.Id.ShouldBe(location2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenLessonNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenLessonNotFound));
        var coach = new Coach("John", "Doe");
        var location = new Location("Studio A");
        dbContext.Coaches.Add(coach);
        dbContext.Locations.Add(location);
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
                location.Id,
                null),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenTypeChanges()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenTypeChanges));
        var coach = new Coach("John", "Doe");
        var location = new Location("Studio A");
        var lesson = new PrivateLesson
        {
            Name = "Lesson",
            Type = LessonType.Private,
            Coach = coach,
            Location = location,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.Coaches.Add(coach);
        dbContext.Locations.Add(location);
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
                location.Id,
                10),
            CancellationToken.None));
    }
}
