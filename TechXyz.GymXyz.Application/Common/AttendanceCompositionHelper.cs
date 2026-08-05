using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The lookups and the guard every attendance command shares. They live here
/// rather than in each handler because "is this sheet still open" has to be
/// answered identically by all four of them — one tap, a whole sheet, a
/// validation and a reopening — and the lock is the whole point of the screen.
/// </summary>
public static class AttendanceCompositionHelper
{
    public static async Task<Session> LoadSessionAsync(
        IGymDbContext dbContext,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .FirstOrDefaultAsync(
                candidate => candidate.Id == sessionId && candidate.IsActive,
                cancellationToken);

        return session ?? throw ValidationFailures.Refuse(
            AttendanceFieldNames.Session,
            AttendanceRules.SessionNotFound);
    }

    /// <summary>
    /// Refuses any write to a sheet that cannot take one: a cancelled session
    /// never had a sheet, and a validated one is closed until a manager reopens
    /// it.
    /// </summary>
    public static void GuardWritable(Session session)
    {
        if (session.Status == SessionStatus.Cancelled)
        {
            throw ValidationFailures.Refuse(
                AttendanceFieldNames.Session,
                AttendanceRules.SessionCancelled);
        }

        if (session.AttendanceClosedAt is not null)
        {
            throw ValidationFailures.Refuse(
                AttendanceFieldNames.Sheet,
                AttendanceRules.SheetClosed);
        }
    }

    /// <summary>
    /// Applies one verdict to one seat, arrival time included.
    /// <para>
    /// The time is cleared for an absence rather than left behind: "dernière
    /// venue" reads it, and a stale check-in would have the member showing up on
    /// a day they were marked away.
    /// </para>
    /// </summary>
    public static void Apply(Registration registration, AttendanceStatus status, DateTime now)
    {
        registration.Status = status;
        registration.CheckedInAt = AttendanceRules.CheckInFor(status, now);

        // Lot 7 — a credit pack is debited here, once Subscription carries
        // CreditsRemaining. Writing it against today's Subscription, which has
        // none of PlanId, CreditsRemaining, AutoRenew or Status, would mean
        // writing it twice.
    }
}
