using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// One session reduced to what a figure is counted from. The screens ask the
/// same four questions of it — how full, how many a week, how many regulars,
/// which day — so it is loaded once and aggregated in memory.
/// </summary>
public sealed record SessionFact(
    int SessionId,
    int LocationId,
    int? CoachId,
    int CourseTemplateId,
    DateTime StartsAt,
    int Capacity,
    int Registered,
    int Present,
    int Late,
    int Absent,
    DateTime? AttendanceClosedAt)
{
    /// <summary>Seats somebody actually pointed. The divider of an attendance rate.</summary>
    public int Marked => Present + Late + Absent;

    /// <summary>Seats pointed as having attended, a late arrival included.</summary>
    public int Attended => Present + Late;

    /// <summary>Whether the sheet has been validated.</summary>
    public bool IsPointed => AttendanceClosedAt is not null;
}

/// <summary>
/// Where every occupancy, fill rate and slot count in the application comes
/// from. Written once so the venue card, the course catalogue and the coach
/// grid cannot disagree about what a busy week means.
/// </summary>
public static class SessionStatistics
{
    /// <summary>
    /// How far back a fill rate looks. Long enough to survive a quiet week,
    /// short enough that last term does not flatter this one.
    /// </summary>
    public const int TrailingWeeks = 4;

    /// <summary>
    /// The sessions a rate is averaged over: the last four weeks up to now.
    /// Cancelled ones are excluded by <see cref="LoadAsync"/> — a class that did
    /// not happen is not a class nobody came to.
    /// </summary>
    public static (DateTime From, DateTime To) TrailingWindow(DateTime now) =>
        (PlanningRules.MondayOf(now).AddDays(-7 * TrailingWeeks), now);

    /// <summary>
    /// How far back an attendance rate looks. Decided before lot 1 and already
    /// documented on the member DTOs: a rolling quarter, long enough that a
    /// fortnight away does not read as having lapsed.
    /// </summary>
    public const int AttendanceWindowDays = 90;

    /// <summary>The sessions an attendance rate is computed over: the trailing quarter.</summary>
    public static (DateTime From, DateTime To) AttendanceWindow(DateTime now) =>
        (now.Date.AddDays(-AttendanceWindowDays), now);

    /// <summary>
    /// The shorter window the Présences screen reads. Its own "taux par cours"
    /// card is labelled "30 derniers jours" in the prototype, and the headline
    /// rate is compared against the thirty days before that to produce the
    /// "+4 pts ce mois" the KPI carries.
    /// <para>
    /// Deliberately not <see cref="AttendanceWindowDays"/>: a member's assiduité
    /// is a standing, which wants a quarter, while this screen is about the month
    /// the manager is running.
    /// </para>
    /// </summary>
    public const int RecentAttendanceDays = 30;

    /// <summary>The trailing month — what the Présences screen counts over.</summary>
    public static (DateTime From, DateTime To) RecentAttendanceWindow(DateTime now) =>
        (now.Date.AddDays(-RecentAttendanceDays), now);

    /// <summary>Monday to Monday of the week in progress — what "cette semaine" counts.</summary>
    public static (DateTime From, DateTime To) CurrentWeek(DateTime now)
    {
        var monday = PlanningRules.MondayOf(now);

        return (monday, monday.AddDays(7));
    }

    /// <summary>
    /// Loads the facts of a window in one query.
    /// <para>
    /// Projected per session and grouped afterwards rather than grouped in SQL:
    /// counting a collection navigation inside a group aggregate is where this
    /// stops translating, and the window is a few hundred rows at most.
    /// </para>
    /// </summary>
    /// <param name="scope">
    /// Whose sessions to count. Defaults to the whole gym, which is right for
    /// every caller drawing a room, a coach or a course — those screens are a
    /// manager's. The Présences figures pass a real scope, so a coach's rate is
    /// their own rather than the gym's average shown under their name.
    /// </param>
    public static async Task<List<SessionFact>> LoadAsync(
        IGymDbContext dbContext,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken,
        CoachScope scope = default)
    {
        return await scope.Apply(dbContext.Sessions.AsNoTracking())
            .Where(session =>
                session.IsActive &&
                session.Status != SessionStatus.Cancelled &&
                session.StartsAt >= from &&
                session.StartsAt < to)
            .Select(session => new SessionFact(
                session.Id,
                session.LocationId,
                session.CoachId,
                session.CourseTemplateId,
                session.StartsAt,
                session.Capacity,
                session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted),
                session.Registrations!.Count(seat =>
                    seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Present),
                session.Registrations!.Count(seat =>
                    seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Late),
                session.Registrations!.Count(seat =>
                    seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Absent),
                session.AttendanceClosedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Seats taken over seats offered, as a percentage. Null when there was
    /// nothing to fill: no session is not an empty one, and the screens show
    /// "—" for it.
    /// <para>
    /// Private sessions are left out. A session that seats one is full the
    /// moment it is booked, so counting it would make every coach who takes
    /// individual clients look in high demand and would flatter the studio they
    /// use — there is no filling to measure.
    /// </para>
    /// </summary>
    public static int? FillRate(IEnumerable<SessionFact> facts)
    {
        var counted = facts.Where(fact => fact.Capacity > 1).ToList();

        return counted.Count == 0
            ? null
            : PlanningRules.FillRate(counted.Sum(fact => fact.Registered), counted.Sum(fact => fact.Capacity));
    }

    /// <summary>
    /// Seats pointed as attended over seats pointed at all, as a percentage.
    /// Null when nothing was marked.
    /// <para>
    /// That null is the point of the method. A sheet nobody opened has every
    /// seat <see cref="AttendanceStatus.Pending"/>, and reading it as nought per
    /// cent attendance would drag every average down for a class that may well
    /// have been full — the same rule <see cref="FillRate"/> follows for a week
    /// with no sessions in it, and the screens show "—" for it.
    /// </para>
    /// <para>
    /// Private sessions are counted here, unlike a fill rate: a one-to-one that
    /// the member missed is exactly the no-show the screen is looking for.
    /// </para>
    /// </summary>
    public static int? AttendanceRate(IEnumerable<SessionFact> facts)
    {
        var counted = facts.ToList();
        var marked = counted.Sum(fact => fact.Marked);

        return marked == 0
            ? null
            : (int)Math.Round(100d * counted.Sum(fact => fact.Attended) / marked);
    }

    /// <summary>
    /// Seven rates, Monday to Sunday — the heatmap. A day without a session
    /// reads zero rather than dropping out: the row has to keep seven cells.
    /// </summary>
    public static IReadOnlyList<int> DailyRates(IEnumerable<SessionFact> weekFacts)
    {
        var byDay = weekFacts
            .GroupBy(fact => ((int)fact.StartsAt.DayOfWeek + 6) % 7)
            .ToDictionary(group => group.Key, group => FillRate(group) ?? 0);

        return [.. Enumerable.Range(0, 7).Select(day => byDay.GetValueOrDefault(day))];
    }
}
