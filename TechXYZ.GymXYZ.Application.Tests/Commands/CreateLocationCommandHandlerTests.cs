using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateStudioWithItsEquipmentInOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldCreateStudioWithItsEquipmentInOrder));

        var site = new Site("GymXYZ Lyon 3ᵉ");
        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var locationId = await handler.Handle(
            new CreateLocationCommand(
                " Studio A ",
                LocationKind.Studio,
                capacity: 20,
                typeLabel: "Cours collectifs",
                areaSqm: 65m,
                floor: "Rez-de-chaussée",
                siteId: site.Id,
                equipment: ["Tapis ×20", "  ", "Steps", "Tapis ×20"]),
            CancellationToken.None);

        locationId.ShouldBeGreaterThan(0);

        var created = dbContext.Locations.Single(location => location.Id == locationId);
        created.Name.ShouldBe("Studio A");
        created.Kind.ShouldBe(LocationKind.Studio);
        created.SiteId.ShouldBe(site.Id);

        // Blanks dropped, duplicates dropped, order kept.
        dbContext.LocationEquipment
            .Where(equipment => equipment.LocationId == locationId)
            .OrderBy(equipment => equipment.Rank)
            .Select(equipment => equipment.Label)
            .ShouldBe(["Tapis ×20", "Steps"]);
    }

    [Fact]
    public async Task Handle_ShouldCreateOutdoorLocationWithItsMeetingPointAndFallback()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldCreateOutdoorLocationWithItsMeetingPointAndFallback));

        var studio = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.Add(studio);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var locationId = await handler.Handle(
            new CreateLocationCommand(
                "Parc de la Tête d'Or",
                LocationKind.Outdoor,
                capacity: 20,
                street: "Entrée Bd des Belges",
                isWeatherDependent: true,
                fallbackLocationId: studio.Id),
            CancellationToken.None);

        var created = dbContext.Locations.Single(location => location.Id == locationId);
        created.IsWeatherDependent.ShouldBeTrue();
        created.FallbackLocationId.ShouldBe(studio.Id);
        created.Address.ShouldNotBeNull();
        created.Address!.Street.ShouldBe("Entrée Bd des Belges");
        created.SiteId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldRejectAHomeLocationSeatingMoreThanOne()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldRejectAHomeLocationSeatingMoreThanOne));

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateLocationCommand("À domicile", LocationKind.Home, capacity: 4),
            CancellationToken.None));

        exception.Message.ShouldContain(LocationRules.HomeCapacityMessage);
    }

    [Fact]
    public async Task Handle_ShouldRejectWeatherDependenceOnAnIndoorLocation()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldRejectWeatherDependenceOnAnIndoorLocation));

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateLocationCommand(
                "Studio A", LocationKind.Studio, capacity: 20, isWeatherDependent: true),
            CancellationToken.None));

        exception.Message.ShouldContain(LocationRules.WeatherKindMessage);
    }

    /// <summary>
    /// Falling back from one park to another shelters nobody, so the fallback
    /// has to be a studio.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldRejectAFallbackThatIsNotAStudio()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldRejectAFallbackThatIsNotAStudio));

        var otherPark = new Location("Parc Blandan") { Kind = LocationKind.Outdoor, Capacity = 20 };
        dbContext.Locations.Add(otherPark);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateLocationCommand(
                "Parc de la Tête d'Or",
                LocationKind.Outdoor,
                capacity: 20,
                fallbackLocationId: otherPark.Id),
            CancellationToken.None));

        exception.Message.ShouldContain(LocationRules.FallbackKindMessage);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenTheSiteDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldThrowValidationException_WhenTheSiteDoesNotExist));

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateLocationCommand("Studio A", LocationKind.Studio, capacity: 20, siteId: 999),
            CancellationToken.None));
    }
}
