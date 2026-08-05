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
    int Registered);

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
    public static async Task<List<SessionFact>> LoadAsync(
        IGymDbContext dbContext,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return await dbContext.Sessions
            .AsNoTracking()
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
                session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted)))
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
