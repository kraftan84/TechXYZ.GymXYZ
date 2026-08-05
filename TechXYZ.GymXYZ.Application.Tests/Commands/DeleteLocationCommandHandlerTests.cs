using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteLocation()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteLocation));
        var location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteLocationCommandHandler(dbContext, new DeleteLocationCommandValidator());

        var deleted = await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Locations.Single(candidate => candidate.Id == location.Id).IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// An archived venue must stop showing up on the records that named it —
    /// the course that proposed it first, and the outdoor venue that fell back
    /// on it when it rained.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReleaseWhoeverPointedAtTheVenue()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldReleaseWhoeverPointedAtTheVenue));

        var studio = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.Add(studio);
        await dbContext.SaveChangesAsync();

        var park = new Location("Parc de la Tête d'Or")
        {
            Kind = LocationKind.Outdoor,
            Capacity = 20,
            IsWeatherDependent = true,
            FallbackLocationId = studio.Id
        };
        var discipline = new Discipline("Cycling");
        var template = new CourseTemplate("Power Cycle")
        {
            Discipline = discipline,
            DurationMinutes = 45,
            Capacity = 24,
            DefaultLocationId = studio.Id
        };

        dbContext.Locations.Add(park);
        dbContext.Disciplines.Add(discipline);
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteLocationCommandHandler(dbContext, new DeleteLocationCommandValidator());

        await handler.Handle(new DeleteLocationCommand(studio.Id), CancellationToken.None);

        dbContext.CourseTemplates.Single(candidate => candidate.Id == template.Id)
            .DefaultLocationId.ShouldBeNull();
        dbContext.Locations.Single(candidate => candidate.Id == park.Id)
            .FallbackLocationId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTheVenueIsUnknown()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldReturnFalse_WhenTheVenueIsUnknown));

        var handler = new DeleteLocationCommandHandler(dbContext, new DeleteLocationCommandValidator());

        var deleted = await handler.Handle(new DeleteLocationCommand(999), CancellationToken.None);

        deleted.ShouldBeFalse();
    }
}
