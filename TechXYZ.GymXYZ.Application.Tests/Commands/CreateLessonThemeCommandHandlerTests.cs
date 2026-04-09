using Shouldly;
using TechXyz.GymXyz.Application.Commands;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateLessonThemeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateTheme()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateTheme));
        var handler = new CreateLessonThemeCommandHandler(dbContext, new CreateLessonThemeCommandValidator());

        var themeId = await handler.Handle(
            new CreateLessonThemeCommand($" {faker.Commerce.Department()} ", faker.Lorem.Sentence()),
            CancellationToken.None);

        var theme = await dbContext.LessonThemes.FindAsync(themeId);
        theme.ShouldNotBeNull();
        theme!.Name.ShouldNotBeNullOrWhiteSpace();
    }
}
