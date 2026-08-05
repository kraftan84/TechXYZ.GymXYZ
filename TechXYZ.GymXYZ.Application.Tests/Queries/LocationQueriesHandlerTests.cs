using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class LocationQueriesHandlerTests
{
    /// <summary>
    /// Grouped by nature, then alphabetical. Ordering on the Kind column would
    /// sort the stored strings — "Home" before "Studio" — and put the member's
    /// living room at the top of the catalogue.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldGroupByKind_ThenSortByName()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetLocations_ShouldGroupByKind_ThenSortByName));

        dbContext.Locations.AddRange(
            NewLocation("À domicile", LocationKind.Home, capacity: 1),
            NewLocation("Studio B", LocationKind.Studio),
            NewLocation("Parc de la Tête d'Or", LocationKind.Outdoor),
            NewLocation("Espace libre", LocationKind.Studio),
            NewLocation("Studio archivé", LocationKind.Studio, isActive: false));
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);

        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        result.Items.Select(item => item.Name)
            .ShouldBe(["Espace libre", "Studio B", "Parc de la Tête d'Or", "À domicile"]);
        result.StudioCount.ShouldBe(2);
        result.OutdoorCount.ShouldBe(1);
        result.HomeCount.ShouldBe(1);
    }

    /// <summary>
    /// The KPI row counts seats available at the same moment, which is not what
    /// a session at the member's home offers.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldLeaveTheHomeVenueOutOfTheTotalCapacity()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetLocations_ShouldLeaveTheHomeVenueOutOfTheTotalCapacity));

        dbContext.Locations.AddRange(
            NewLocation("Studio A", LocationKind.Studio, capacity: 20),
            NewLocation("Parc de la Tête d'Or", LocationKind.Outdoor, capacity: 20),
            NewLocation("À domicile", LocationKind.Home, capacity: 1));
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);

        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        result.TotalCapacity.ShouldBe(40);
        result.TotalCount.ShouldBe(3);
    }

    /// <summary>
    /// Occupancy, weekly slots and the heatmap are all counted from sessions.
    /// Until the planning produces them the screen shows "—", and nothing here
    /// invents a figure to fill the gap.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldLeaveTheFiguresUnset_WhenNothingIsBookedThere()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetLocations_ShouldLeaveTheFiguresUnset_WhenNothingIsBookedThere));

        dbContext.Locations.Add(NewLocation("Studio A", LocationKind.Studio));
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);

        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        result.Items.Single().OccupancyRate.ShouldBeNull();
        result.Items.Single().SessionsPerWeek.ShouldBeNull();
        result.AverageStudioOccupancy.ShouldBeNull();
        result.TotalSessionsPerWeek.ShouldBeNull();
    }

    [Fact]
    public async Task GetLocationDetails_ShouldReturnEquipmentInOrder_AndAnEmptySchedule()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetLocationDetails_ShouldReturnEquipmentInOrder_AndAnEmptySchedule));

        var site = new Site("GymXYZ Lyon 3ᵉ");
        var studio = NewLocation("Studio A", LocationKind.Studio);
        studio.AddEquipment("Tapis ×20", 1);
        studio.AddEquipment("Sono", 0);
        site.AddLocation(studio);

        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationDetailsPageQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetLocationDetailsPageQuery(studio.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.SiteName.ShouldBe("GymXYZ Lyon 3ᵉ");
        result.Equipment.ShouldBe(["Sono", "Tapis ×20"]);
        result.Today.ShouldBeEmpty();
        result.Occupancy.HasHeatmap.ShouldBeFalse();
        result.Occupancy.AverageRate.ShouldBeNull();
    }

    [Fact]
    public async Task GetLocationDetails_ShouldReturnNull_ForAnArchivedVenue()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetLocationDetails_ShouldReturnNull_ForAnArchivedVenue));

        var archived = NewLocation("Studio A", LocationKind.Studio, isActive: false);
        dbContext.Locations.Add(archived);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationDetailsPageQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetLocationDetailsPageQuery(archived.Id), CancellationToken.None);

        result.ShouldBeNull();
    }

    /// <summary>
    /// The catalogue projection runs against a real engine: the fallback name,
    /// the address and the ordered equipment are all read through navigations,
    /// and a projection that passes in memory can still fail to translate.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldProjectEveryNavigation_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var studio = NewLocation("Studio A", LocationKind.Studio, capacity: 20);
        studio.AreaSqm = 65m;
        studio.Floor = "Rez-de-chaussée";
        studio.AddEquipment("Tapis ×20", 0);
        studio.AddEquipment("Steps", 1);
        dbContext.Locations.Add(studio);
        await dbContext.SaveChangesAsync();

        var park = NewLocation("Parc de la Tête d'Or", LocationKind.Outdoor, capacity: 20);
        park.IsWeatherDependent = true;
        park.FallbackLocationId = studio.Id;
        park.Address = new Address
        {
            Street = "Entrée Bd des Belges",
            ZipCode = string.Empty,
            City = string.Empty,
            Country = string.Empty
        };
        dbContext.Locations.Add(park);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);

        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        var savedStudio = result.Items.Single(item => item.Name == "Studio A");
        savedStudio.Equipment.ShouldBe(["Tapis ×20", "Steps"]);
        savedStudio.AreaSqm.ShouldBe(65m);
        savedStudio.Status.ShouldBe(LocationStatus.Available);

        var savedPark = result.Items.Single(item => item.Name == "Parc de la Tête d'Or");
        savedPark.FallbackLocationName.ShouldBe("Studio A");
        savedPark.Address!.Street.ShouldBe("Entrée Bd des Belges");
        savedPark.Status.ShouldBe(LocationStatus.WeatherDependent);
    }

    private static Location NewLocation(
        string name,
        LocationKind kind,
        int capacity = 20,
        bool isActive = true)
        => new(name)
        {
            Kind = kind,
            Capacity = capacity,
            IsActive = isActive
        };
}
