using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// One member's assiduité: how well they turned up over the window, and when
/// they were last in the building.
/// </summary>
public sealed record MemberAttendanceFact(int MemberId, int Attended, int Marked, DateTime? LastVisitOn)
{
    /// <summary>
    /// Null when none of their seats was ever pointed. A member nobody has
    /// pointed has no assiduité, which is not the same as a bad one, and the
    /// column shows "—" for it.
    /// </summary>
    public int? Rate => Marked == 0 ? null : (int)Math.Round(100d * Attended / Marked);

    public DateOnly? LastVisitOnDate =>
        LastVisitOn is null ? null : DateOnly.FromDateTime(LastVisitOn.Value);
}

/// <summary>
/// Where the "assiduité" and "dernière venue" columns come from — the two
/// figures lot 1 shipped as "—" because nothing wrote attendance yet.
/// <para>
/// Written once because the members table and the member record both ask it,
/// and a member whose row says 78 % must not open on a card that says
/// something else.
/// </para>
/// </summary>
public static class MemberAttendance
{
    /// <summary>
    /// Loads the assiduité of the members given, keyed by member id. A member
    /// with nothing pointed is absent from the dictionary rather than present
    /// with a zero.
    /// <para>
    /// The rate is bounded by <paramref name="from"/>..<paramref name="to"/> —
    /// a rolling quarter, decided before lot 1 — but the last visit is not: a
    /// member who last came four months ago should read "il y a 4 mois", not
    /// "—". The window is about how they have been doing lately; the last visit
    /// is a fact.
    /// </para>
    /// <para>
    /// The seats are pulled over and grouped in memory rather than aggregated in
    /// SQL, the same way <c>SessionStatistics.LoadAsync</c> does: counting per
    /// member and per status inside one group aggregate is where this stops
    /// translating on a relational provider.
    /// </para>
    /// </summary>
    public static async Task<Dictionary<int, MemberAttendanceFact>> LoadAsync(
        IGymDbContext dbContext,
        IReadOnlyCollection<int> memberIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (memberIds.Count == 0)
        {
            return [];
        }

        var seats = await dbContext.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.IsActive &&
                !registration.IsWaitlisted &&
                memberIds.Contains(registration.MemberId) &&
                registration.Session!.IsActive &&
                registration.Session.Status != SessionStatus.Cancelled &&
                registration.Session.StartsAt < to)
            .Select(registration => new
            {
                registration.MemberId,
                registration.Status,
                registration.CheckedInAt,
                registration.Session!.StartsAt
            })
            .ToListAsync(cancellationToken);

        return seats
            .GroupBy(seat => seat.MemberId)
            .Select(group =>
            {
                var inWindow = group.Where(seat => seat.StartsAt >= from).ToList();

                return new MemberAttendanceFact(
                    group.Key,
                    inWindow.Count(seat => AttendanceRules.CountsAsAttended(seat.Status)),
                    inWindow.Count(seat => AttendanceRules.IsMarked(seat.Status)),
                    group.Max(seat => seat.CheckedInAt));
            })
            .Where(fact => fact.Marked > 0 || fact.LastVisitOn is not null)
            .ToDictionary(fact => fact.MemberId);
    }
}
