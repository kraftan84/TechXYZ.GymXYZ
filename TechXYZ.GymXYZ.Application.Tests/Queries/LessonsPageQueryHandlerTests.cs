using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class LessonsPageQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveData()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnOnlyActiveData));
        var coach = new Coach("John", "Doe");
        var inactiveCoach = new Coach("Inactive", "Coach") { IsActive = false };
        var room = new Room("Studio A");
        var inactiveRoom = new Room("Studio B") { IsActive = false };
        var theme = new LessonTheme("Cardio");
        var inactiveTheme = new LessonTheme("Old theme") { IsActive = false };

        var privateLesson = new PrivateLesson
        {
            Name = "Private lesson",
            Type = LessonType.Private,
            Coach = coach,
            Theme = theme,
            Room = room,
            StartDate = DateTime.UtcNow.Date.AddHours(8),
            EndDate = DateTime.UtcNow.Date.AddHours(9)
        };

        var collectiveLesson = new CollectiveLesson
        {
            Name = "Collective lesson",
            Type = LessonType.Collective,
            Coach = coach,
            Theme = theme,
            Rooms = new List<Room> { room, inactiveRoom },
            MaxParticipants = 20,
            StartDate = DateTime.UtcNow.Date.AddHours(10),
            EndDate = DateTime.UtcNow.Date.AddHours(11)
        };

        var inactiveLesson = new PrivateLesson
        {
            Name = "Inactive lesson",
            Type = LessonType.Private,
            Coach = inactiveCoach,
            Theme = inactiveTheme,
            Room = room,
            IsActive = false,
            StartDate = DateTime.UtcNow.Date.AddHours(12),
            EndDate = DateTime.UtcNow.Date.AddHours(13)
        };

        dbContext.Coaches.AddRange(coach, inactiveCoach);
        dbContext.Rooms.AddRange(room, inactiveRoom);
        dbContext.LessonThemes.AddRange(theme, inactiveTheme);
        dbContext.PrivateLessons.AddRange(privateLesson, inactiveLesson);
        dbContext.CollectiveLessons.Add(collectiveLesson);
        await dbContext.SaveChangesAsync();

        var handler = new GetLessonsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetLessonsPageQuery(), CancellationToken.None);

        result.Lessons.Count.ShouldBe(2);
        result.Themes.Count.ShouldBe(1);
        result.Coaches.Count.ShouldBe(1);
        result.Rooms.Count.ShouldBe(1);
        result.Lessons.Single(lesson => lesson.Name == "Collective lesson").Rooms.Count.ShouldBe(1);
    }
}
