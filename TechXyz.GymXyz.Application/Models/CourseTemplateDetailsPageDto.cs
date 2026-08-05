using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

public sealed record CourseTemplateDetailsPageDto(
    int Id,
    string Name,
    int DisciplineId,
    string DisciplineName,
    string? DisciplineIconKey,
    string? IconKeyOverride,
    int DurationMinutes,
    int Capacity,
    int? DefaultLocationId,
    string? DefaultLocationName,
    CourseLevel Level,
    CourseIntensity Intensity,
    decimal? Price,
    string? Description,
    List<CourseTemplateCoachDto> Coaches,
    List<CourseSessionDto> NextSessions,
    CourseTemplateStatsDto Stats)
{
    /// <summary>The course icon, falling back to its discipline's.</summary>
    public string? IconKey => IconKeyOverride ?? DisciplineIconKey;

    /// <summary>A course that seats one is a private lesson.</summary>
    public bool IsPrivate => Capacity == 1;
}

/// <summary>
/// Figures on the record. All three are counted from past occurrences, so they
/// stay unset until lot 5 and are shown as "—".
/// </summary>
public sealed record CourseTemplateStatsDto(
    int? SessionsPerWeek,
    int? FillRate,
    int? Regulars)
{
    public static CourseTemplateStatsDto Empty { get; } = new(null, null, null);
}

/// <summary>Shape of the "Prochaines séances" list. Filled at lot 5.</summary>
public sealed record CourseSessionDto(
    string DayLabel,
    string Time,
    string LocationName,
    int Occupancy,
    int Capacity);
