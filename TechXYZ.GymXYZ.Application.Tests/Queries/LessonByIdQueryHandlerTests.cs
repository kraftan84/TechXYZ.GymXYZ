using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class LessonByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnLesson_WhenItExists()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnLesson_WhenItExists));
        var coach = new Coach("John", "Doe");
        var location = new Location("Studio A");
        var theme = new LessonTheme("Cardio");
        var lesson = new CollectiveLesson
        {
            Name = "Collective lesson",
            Type = LessonType.Collective,
            Coach = coach,
            Theme = theme,
            Locations = new List<Location> { location },
            MaxParticipants = 20,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        dbContext.Coaches.Add(coach);
        dbContext.Locations.Add(location);
        dbContext.LessonThemes.Add(theme);
        dbContext.CollectiveLessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        var handler = new GetLessonByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetLessonByIdQuery(lesson.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Name.ShouldBe("Collective lesson");
        result.Locations.Count.ShouldBe(1);
        result.MaxParticipants.ShouldBe(20);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenLessonDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnNull_WhenLessonDoesNotExist));
        var handler = new GetLessonByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetLessonByIdQuery(999), CancellationToken.None);

        result.ShouldBeNull();
    }
}
