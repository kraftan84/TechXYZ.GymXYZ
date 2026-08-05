using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The Présences screen in one read: what still needs pointing, what has been
/// pointed, and what the assiduité looks like around it.
/// </summary>
public sealed record AttendanceOverviewDto(
    DateOnly Day,
    AttendanceKpisDto Kpis,
    IReadOnlyList<AttendanceSessionDto> ToPoint,
    IReadOnlyList<AttendanceSessionDto> Pointed,
    IReadOnlyList<CourseAttendanceDto> CourseRates,
    IReadOnlyList<AbsentMemberDto> ToChase)
{
    public static AttendanceOverviewDto Empty { get; } =
        new(default, AttendanceKpisDto.Empty, [], [], [], []);
}

/// <summary>
/// The four figures across the top. Every one of them is null-safe about having
/// nothing to count: a rate with no marked seat behind it is null, not nought.
/// </summary>
public sealed record AttendanceKpisDto(
    int? AttendanceRate,
    int? AttendanceDeltaPoints,
    int SheetsToPoint,
    int PresentToday,
    int SessionsToday,
    int NoShowsThisWeek)
{
    public static AttendanceKpisDto Empty { get; } = new(null, null, 0, 0, 0, 0);
}

/// <summary>
/// One session as the two lists draw it: the heading, and where its sheet
/// stands.
/// </summary>
public sealed record AttendanceSessionDto(
    int Id,
    DateTime StartsAt,
    DateTime EndsAt,
    string CourseName,
    string? CoachFirstName,
    string? CoachLastName,
    string LocationName,
    int Registered,
    int Present,
    int Late,
    int Absent,
    DateTime? AttendanceClosedAt)
{
    /// <summary>Seats somebody pointed, either way.</summary>
    public int Marked => Present + Late + Absent;

    /// <summary>Seats pointed as having attended, a late arrival included.</summary>
    public int Attended => Present + Late;

    /// <summary>Seats nobody has reached yet.</summary>
    public int Pending => Registered - Marked;

    /// <summary>« pointée » — the sheet has been validated.</summary>
    public bool IsPointed => AttendanceClosedAt is not null;

    /// <summary>« en cours » — the clock is inside the slot. Derived, never stored.</summary>
    public bool IsLive(DateTime now) => !IsPointed && now >= StartsAt && now < EndsAt;

    /// <summary>Null while nothing has been pointed, so the row shows "—" rather than 0 %.</summary>
    public int? AttendanceRate => Marked == 0 ? null : (int)Math.Round(100d * Attended / Marked);

    public string? CoachFullName => CoachLastName is null
        ? null
        : $"{CoachFirstName} {CoachLastName}".Trim();
}

/// <summary>One bar of "Taux par cours".</summary>
public sealed record CourseAttendanceDto(int CourseTemplateId, string CourseName, int Rate);

/// <summary>
/// One line of "Absents à relancer": how many sessions the member was booked on
/// and missed over the window, and when they last actually came.
/// </summary>
public sealed record AbsentMemberDto(
    int MemberId,
    string FirstName,
    string LastName,
    int Missed,
    int Booked,
    DateTime? LastVisitOn)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// The window this is counted over, so the card can say so without
    /// hard-coding a number the query owns.
    /// </summary>
    public static int WindowDays => SessionStatistics.RecentAttendanceDays;
}
