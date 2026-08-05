using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteLessonThemeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteThemeAndUnlinkLessons()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteThemeAndUnlinkLessons));
        var theme = new LessonTheme("Cardio");
        var coach = new Coach("John", "Doe");
        var location = new Location("Studio A");
        var lesson = new PrivateLesson
        {
            Name = "Lesson",
            Type = LessonType.Private,
            Theme = theme,
            Coach = coach,
            Location = location,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.LessonThemes.Add(theme);
        dbContext.Coaches.Add(coach);
        dbContext.Locations.Add(location);
        dbContext.PrivateLessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteLessonThemeCommandHandler(dbContext, new DeleteLessonThemeCommandValidator());
        var result = await handler.Handle(new DeleteLessonThemeCommand(theme.Id), CancellationToken.None);

        result.ShouldBeTrue();
        theme.IsActive.ShouldBeFalse();

        var persistedLesson = await dbContext.PrivateLessons
            .AsNoTracking()
            .Include(candidate => candidate.Theme)
            .FirstAsync(candidate => candidate.Id == lesson.Id);
        persistedLesson.Theme.ShouldBeNull();
    }
}
