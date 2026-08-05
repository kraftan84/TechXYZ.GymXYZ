using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One week of the planning, Monday to Sunday. The grid draws seven columns of
/// hours, so the week it covers travels with the sessions rather than being
/// recomputed on the other side.
/// </summary>
public sealed record WeekPlanningDto(
    DateOnly WeekStart,
    IReadOnlyList<PlanningSessionDto> Sessions)
{
    public static WeekPlanningDto Empty { get; } = new(default, []);

    public DateOnly WeekEnd => WeekStart.AddDays(6);

    /// <summary>Seats taken across the week, against the seats offered.</summary>
    public int TotalRegistered => Sessions.Sum(session => session.Registered);
    public int TotalCapacity => Sessions.Sum(session => session.Capacity);

    /// <summary>The sessions of one day, in order — what the day view and the mobile agenda read.</summary>
    public IReadOnlyList<PlanningSessionDto> On(DateOnly day) =>
        Sessions.Where(session => DateOnly.FromDateTime(session.StartsAt) == day).ToList();
}

/// <summary>
/// One block of the grid: what it is, who runs it, and how full it is. The coach
/// is abbreviated the way the prototype writes it — "N. Lemoine".
/// </summary>
public sealed record PlanningSessionDto(
    int Id,
    DateTime StartsAt,
    DateTime EndsAt,
    string CourseName,
    int CourseTemplateId,
    string? CoachFirstName,
    string? CoachLastName,
    int? CoachId,
    string LocationName,
    int LocationId,
    int Registered,
    int Waitlisted,
    int Capacity,
    SessionStatus Status,
    bool IsRecurring,
    DateTime? AttendanceClosedAt)
{
    /// <summary>Its attendance sheet has been validated — what the day view counts.</summary>
    public bool IsPointed => AttendanceClosedAt is not null;

    /// <summary>Monday is 0, which is the order the seven columns are drawn in.</summary>
    public int DayIndex => ((int)StartsAt.DayOfWeek + 6) % 7;

    /// <summary>The hour row the block sits on.</summary>
    public int Hour => StartsAt.Hour;

    /// <summary>A session that seats one is private — there is no type to keep in step.</summary>
    public bool IsPrivate => Capacity == 1;

    public bool IsFull => Registered >= Capacity && Capacity > 1;

    public bool IsCancelled => Status == SessionStatus.Cancelled;

    /// <summary>"N. Lemoine", or null when nobody animates the slot.</summary>
    public string? CoachShortName => CoachLastName is null
        ? null
        : string.IsNullOrEmpty(CoachFirstName)
            ? CoachLastName
            : $"{CoachFirstName[0]}. {CoachLastName}";

    public string? CoachFullName => CoachLastName is null
        ? null
        : $"{CoachFirstName} {CoachLastName}".Trim();
}
