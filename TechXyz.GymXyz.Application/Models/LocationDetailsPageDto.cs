using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The venue record. Half of what the prototype draws on it — the day's
/// schedule and the weekly heatmap — is read from sessions, so both come back
/// empty until lot 5 and the cards say so rather than showing zeroes.
/// </summary>
public sealed record LocationDetailsPageDto(
    int Id,
    string Name,
    LocationKind Kind,
    string? TypeLabel,
    string? IconKey,
    string? Tone,
    int Capacity,
    decimal? AreaSqm,
    string? Floor,
    string? Note,
    bool IsOpenAccess,
    bool IsWeatherDependent,
    int? SiteId,
    string? SiteName,
    int? FallbackLocationId,
    string? FallbackLocationName,
    AddressDto? Address,
    List<string> Equipment,
    List<LocationSessionDto> Today,
    LocationOccupancyDto Occupancy)
{
    public LocationStatus Status => LocationStatusRules.Resolve(Kind, IsOpenAccess, IsWeatherDependent);
}

/// <summary>One line of "Planning du jour". Filled at lot 5.</summary>
public sealed record LocationSessionDto(
    string Time,
    string CourseName,
    string? CoachName,
    int Registered,
    int Capacity)
{
    /// <summary>A session seating one is a private lesson, chipped "Privé".</summary>
    public bool IsPrivate => Capacity == 1;
}

/// <summary>
/// The venue's occupancy figures: the average shown on the card, the weekly
/// slot count, and the seven daily rates of the heatmap. Every one of them is
/// counted from sessions, so an empty instance is what lot 4 can honestly
/// answer.
/// </summary>
public sealed record LocationOccupancyDto(
    int? AverageRate,
    int? SessionsPerWeek,
    IReadOnlyList<int> DailyRates)
{
    public static LocationOccupancyDto Empty { get; } = new(null, null, []);

    /// <summary>Seven values, Monday to Sunday, or nothing to draw at all.</summary>
    public bool HasHeatmap => DailyRates.Count == 7;
}
