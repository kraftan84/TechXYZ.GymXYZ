namespace TechXyz.GymXyz.Application.Models;

public sealed record CoachDetailsPageDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? RoleLabel,
    string? Bio,
    DateOnly JoinedOn,
    DateOnly? AwayUntil,
    AddressDto? Address,
    WeeklyAvailabilityDto Availability,
    List<DisciplineDto> Disciplines,
    List<string> Certifications,
    List<CoachSessionDto> WeekSessions,
    CoachStatsDto Stats)
{
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Set by the query handler from <c>CoachStatusRules</c>.</summary>
    public CoachStatus Status { get; init; }

    /// <summary>
    /// True when the coach signs in with an Identity account. The account
    /// itself cannot be read from here: <c>Application</c> does not see
    /// <c>ApplicationUser</c>, and the directory abstraction that fixes it is
    /// scheduled for lot 8.
    /// </summary>
    public bool HasAccount { get; init; }
}

/// <summary>
/// Weekly availability, Monday to Sunday. Seven members rather than a list so
/// the whole thing projects server-side in one pass.
/// </summary>
public sealed record WeeklyAvailabilityDto(
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday)
{
    public static WeeklyAvailabilityDto None { get; } = new(false, false, false, false, false, false, false);

    /// <summary>Monday first, the way the strip is drawn.</summary>
    public IReadOnlyList<bool> Days => [Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday];

    public int AvailableDayCount => Days.Count(day => day);
}

/// <summary>
/// Figures on the record, counted from the sessions the coach runs. They stay
/// unset for a coach who runs none, and are shown as "—".
/// </summary>
public sealed record CoachStatsDto(
    int? ClassesPerWeek,
    int? FillRate,
    int? FollowedMembers)
{
    public static CoachStatsDto Empty { get; } = new(null, null, null);
}

/// <summary>One line of "Cours animés cette semaine".</summary>
public sealed record CoachSessionDto(
    string DayLabel,
    string Time,
    string CourseName,
    int Occupancy,
    int Capacity);
