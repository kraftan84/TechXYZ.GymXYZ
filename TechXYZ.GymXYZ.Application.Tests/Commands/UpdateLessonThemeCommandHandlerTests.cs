using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateLessonThemeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateTheme()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateTheme));
        var theme = new LessonTheme("Cardio");
        dbContext.LessonThemes.Add(theme);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLessonThemeCommandHandler(dbContext, new UpdateLessonThemeCommandValidator());
        var result = await handler.Handle(new UpdateLessonThemeCommand(theme.Id, "Mobility", "desc"), CancellationToken.None);

        result.ShouldBeTrue();
        theme.Name.ShouldBe("Mobility");
        theme.Description.ShouldBe("desc");
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenThemeNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenThemeNotFound));
        var handler = new UpdateLessonThemeCommandHandler(dbContext, new UpdateLessonThemeCommandValidator());

        var result = await handler.Handle(new UpdateLessonThemeCommand(999, "Mobility", null), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
