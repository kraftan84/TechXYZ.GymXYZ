namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Where a session stands. Both values are written by the planning.
/// <para>
/// There is deliberately no "live" or "done" value. The screens do show those
/// three states — « à pointer », « en cours », « pointée » — but they are
/// derived, not stored: "en cours" is the clock falling inside
/// <c>StartsAt</c>..<c>EndsAt</c>, and "pointée" is
/// <c>Session.AttendanceClosedAt</c> being set. Stored, they would need a sweep
/// to keep them true and would be wrong every minute in between.
/// </para>
/// </summary>
public enum SessionStatus
{
    Scheduled,
    Cancelled
}
