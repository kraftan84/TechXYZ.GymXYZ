using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The venue catalogue plus the four figures of its KPI row. No paging and no
/// search: the prototype shows neither, and a gym is counted in units of venues.
/// </summary>
public sealed record LocationsPageDto(
    IReadOnlyList<LocationListItemDto> Items,
    int StudioCount,
    int OutdoorCount,
    int HomeCount)
{
    public static LocationsPageDto Empty { get; } = new([], 0, 0, 0);

    public int TotalCount => Items.Count;

    /// <summary>
    /// Seats available at the same moment. The home kind is left out: its single
    /// seat is at the member's place, not somewhere the gym can fill.
    /// </summary>
    public int TotalCapacity => Items
        .Where(item => item.Kind != LocationKind.Home)
        .Sum(item => item.Capacity);

    /// <summary>
    /// Average occupancy over the studios, or null while no session has been
    /// held. Counted from sessions, so it stays null until lot 5.
    /// </summary>
    public int? AverageStudioOccupancy
    {
        get
        {
            var rates = Items
                .Where(item => item.Kind == LocationKind.Studio && item.OccupancyRate.HasValue)
                .Select(item => item.OccupancyRate!.Value)
                .ToList();

            return rates.Count == 0 ? null : (int)Math.Round(rates.Average());
        }
    }

    /// <summary>Slots a week across every venue. Null until the planning fills it.</summary>
    public int? TotalSessionsPerWeek
    {
        get
        {
            var counts = Items.Where(item => item.SessionsPerWeek.HasValue).ToList();

            return counts.Count == 0 ? null : counts.Sum(item => item.SessionsPerWeek!.Value);
        }
    }
}

/// <summary>
/// One card of the grid. Occupancy and sessions per week come from past and
/// booked sessions, so they are nullable and left unset — they render as "—"
/// rather than being invented.
/// </summary>
public sealed record LocationListItemDto(
    int Id,
    string Name,
    LocationKind Kind,
    string? TypeLabel,
    string? IconKey,
    string? Tone,
    int Capacity,
    decimal? AreaSqm,
    string? Floor,
    bool IsOpenAccess,
    bool IsWeatherDependent,
    string? FallbackLocationName,
    AddressDto? Address,
    List<string> Equipment)
{
    public LocationStatus Status => LocationStatusRules.Resolve(Kind, IsOpenAccess, IsWeatherDependent);

    /// <summary>Average fill of the venue's sessions, 0–100. Filled at lot 5.</summary>
    public int? OccupancyRate { get; init; }

    /// <summary>Slots booked in a normal week. Filled at lot 5.</summary>
    public int? SessionsPerWeek { get; init; }
}
