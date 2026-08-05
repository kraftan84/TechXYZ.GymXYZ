using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRenameAndMoveToAnotherSite()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldRenameAndMoveToAnotherSite));

        var source = new Site("Source");
        var target = new Site("Target");
        var location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        source.AddLocation(location);

        dbContext.Sites.AddRange(source, target);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLocationCommandHandler(dbContext, new UpdateLocationCommandValidator());

        var updated = await handler.Handle(
            new UpdateLocationCommand(
                location.Id,
                " Studio Alpha ",
                LocationKind.Studio,
                capacity: 24,
                siteId: target.Id),
            CancellationToken.None);

        updated.ShouldBeTrue();

        var saved = dbContext.Locations.Single(candidate => candidate.Id == location.Id);
        saved.Name.ShouldBe("Studio Alpha");
        saved.Capacity.ShouldBe(24);
        saved.SiteId.ShouldBe(target.Id);
    }

    /// <summary>
    /// The list is replaced wholesale, so an edit that drops a line drops the
    /// row, and the ranks close back up.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReplaceTheEquipmentList()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldReplaceTheEquipmentList));

        var location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        location.AddEquipment("Tapis ×20", 0);
        location.AddEquipment("Steps", 1);
        location.AddEquipment("Sono", 2);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLocationCommandHandler(dbContext, new UpdateLocationCommandValidator());

        await handler.Handle(
            new UpdateLocationCommand(
                location.Id,
                "Studio A",
                LocationKind.Studio,
                capacity: 20,
                equipment: ["Sono", "Miroir mural"]),
            CancellationToken.None);

        dbContext.LocationEquipment
            .Where(equipment => equipment.LocationId == location.Id)
            .OrderBy(equipment => equipment.Rank)
            .Select(equipment => equipment.Label)
            .ShouldBe(["Sono", "Miroir mural"]);
    }

    [Fact]
    public async Task Handle_ShouldRejectAVenueFallingBackOnItself()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldRejectAVenueFallingBackOnItself));

        var park = new Location("Parc de la Tête d'Or") { Kind = LocationKind.Outdoor, Capacity = 20 };
        dbContext.Locations.Add(park);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLocationCommandHandler(dbContext, new UpdateLocationCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new UpdateLocationCommand(
                park.Id,
                "Parc de la Tête d'Or",
                LocationKind.Outdoor,
                capacity: 20,
                fallbackLocationId: park.Id),
            CancellationToken.None));

        exception.Message.ShouldContain(LocationRules.FallbackSelfMessage);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTheVenueIsUnknown()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldReturnFalse_WhenTheVenueIsUnknown));

        var handler = new UpdateLocationCommandHandler(dbContext, new UpdateLocationCommandValidator());

        var updated = await handler.Handle(
            new UpdateLocationCommand(999, "Studio A", LocationKind.Studio, capacity: 20),
            CancellationToken.None);

        updated.ShouldBeFalse();
    }
}
