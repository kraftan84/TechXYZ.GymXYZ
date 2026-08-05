using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The course catalogue plus the counts behind the filter chips. No paging: the
/// prototype shows a plain table, and a catalogue is counted in tens.
/// </summary>
public sealed record CourseTemplatesPageDto(
    IReadOnlyList<CourseTemplateListItemDto> Items,
    int TotalCount,
    int CollectiveCount,
    int PrivateCount)
{
    public static CourseTemplatesPageDto Empty { get; } = new([], 0, 0, 0);
}

/// <summary>
/// One row of the table. The average fill comes from past occurrences, so it is
/// null for a course the planning has never run — "—" rather than a zero.
/// </summary>
public sealed record CourseTemplateListItemDto(
    int Id,
    string Name,
    string DisciplineName,
    string? DisciplineIconKey,
    string? IconKeyOverride,
    int DurationMinutes,
    int Capacity,
    CourseLevel Level,
    CourseIntensity Intensity,
    List<CourseTemplateCoachDto> Coaches)
{
    /// <summary>The course icon, falling back to its discipline's.</summary>
    public string? IconKey => IconKeyOverride ?? DisciplineIconKey;

    /// <summary>A course that seats one is a private lesson.</summary>
    public bool IsPrivate => Capacity == 1;

    /// <summary>
    /// Average fill of the sessions run over the trailing weeks, 0–100. Null for
    /// a course that has never run.
    /// </summary>
    public int? FillRate { get; init; }
}

/// <summary>A coach allowed to run the course, in avatar-stack order.</summary>
public sealed record CourseTemplateCoachDto(
    int Id,
    string FirstName,
    string LastName,
    string? RoleLabel)
{
    public string FullName => $"{FirstName} {LastName}";
}
