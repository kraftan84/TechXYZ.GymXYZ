using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One attendance sheet: the session it belongs to, the band of four stats, and
/// the people on it.
/// </summary>
public sealed record SessionRosterDto(
    int SessionId,
    DateTime StartsAt,
    DateTime EndsAt,
    string CourseName,
    int CourseTemplateId,
    string? CoachFirstName,
    string? CoachLastName,
    string LocationName,
    int Capacity,
    bool IsCancelled,
    DateTime? AttendanceClosedAt,
    string? AttendanceReopenedBy,
    DateTime? AttendanceReopenedAt,
    bool CanReopen,
    IReadOnlyList<RosterSeatDto> Seats)
{
    public static SessionRosterDto Empty { get; } =
        new(0, default, default, string.Empty, 0, null, null, string.Empty, 0, false,
            null, null, null, false, []);

    /// <summary>Seats that count — the waiting list is listed apart, never pointed.</summary>
    public IReadOnlyList<RosterSeatDto> Registered =>
        Seats.Where(seat => !seat.IsWaitlisted).ToList();

    public int Present => Registered.Count(seat => seat.Status == AttendanceStatus.Present);
    public int Late => Registered.Count(seat => seat.Status == AttendanceStatus.Late);
    public int Absent => Registered.Count(seat => seat.Status == AttendanceStatus.Absent);
    public int Pending => Registered.Count(seat => seat.Status == AttendanceStatus.Pending);

    public int Marked => Present + Late + Absent;
    public int Attended => Present + Late;

    /// <summary>Null while nothing has been pointed: the band shows "—", not 0 %.</summary>
    public int? AttendanceRate => Marked == 0 ? null : (int)Math.Round(100d * Attended / Marked);

    /// <summary>The sheet has been validated and is read-only.</summary>
    public bool IsClosed => AttendanceClosedAt is not null;

    /// <summary>« en cours » — derived from the clock, never stored.</summary>
    public bool IsLive(DateTime now) => !IsClosed && now >= StartsAt && now < EndsAt;

    /// <summary>A sheet cannot be validated before the session has started.</summary>
    public bool CanClose(DateTime now) => !IsClosed && !IsCancelled && now >= StartsAt;

    public string? CoachFullName => CoachLastName is null
        ? null
        : $"{CoachFirstName} {CoachLastName}".Trim();
}

/// <summary>One line of the sheet: who, on what plan, and what was recorded.</summary>
public sealed record RosterSeatDto(
    int RegistrationId,
    int MemberId,
    string FirstName,
    string LastName,
    bool IsWaitlisted,
    AttendanceStatus Status,
    DateTime? CheckedInAt)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Subscription plan name — the prototype's "Illimité", "Carte 10",
    /// "Étudiant". Filled at lot 7 (Abonnements), where <c>Subscription</c> gains
    /// a plan; today it holds only a session count, and naming a plan from that
    /// would be inventing one. Rendered "—", the same as the column of the same
    /// name on the members table.
    /// </summary>
    public string? PlanLabel { get; init; }
}
