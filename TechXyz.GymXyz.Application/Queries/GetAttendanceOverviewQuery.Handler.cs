using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetAttendanceOverviewQueryHandler
    : IRequestHandler<GetAttendanceOverviewQuery, AttendanceOverviewDto>
{
    /// <summary>How many validated sheets "Séances récentes" lists.</summary>
    private const int RecentSheetCount = 10;

    /// <summary>How many names the "Absents à relancer" card carries.</summary>
    private const int ChaseListSize = 5;

    /// <summary>Below this, an absence is a bad week rather than a habit.</summary>
    private const int ChaseThreshold = 2;

    private readonly IGymDbContext _dbContext;

    public GetAttendanceOverviewQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AttendanceOverviewDto> Handle(
        GetAttendanceOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var today = now.Date;

        var (recentFrom, recentTo) = SessionStatistics.RecentAttendanceWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        var recent = await SessionStatistics.LoadAsync(_dbContext, recentFrom, recentTo, cancellationToken);

        // The month before the one on screen, so the KPI can say which way it is
        // going rather than just where it stands.
        var previous = await SessionStatistics.LoadAsync(
            _dbContext,
            recentFrom.AddDays(-SessionStatistics.RecentAttendanceDays),
            recentFrom,
            cancellationToken);

        var toPoint = await LoadToPointAsync(now, cancellationToken);

        // Counted off the list itself rather than off the facts. The facts stop
        // at "now" — that is what a rate is averaged over — so an evening class
        // that has not started yet would be missing from the KPI while sitting
        // in the list right underneath it, and the screen would show two answers
        // to the same question.
        var overdue = toPoint.Count(session => session.StartsAt.Date < today);

        // Today's classes, for the two figures the prototype labels "aujourd'hui".
        var todaysFacts = recent.Where(fact => fact.StartsAt.Date == today).ToList();

        var kpis = new AttendanceKpisDto(
            SessionStatistics.AttendanceRate(recent),
            AttendanceDelta(recent, previous),
            toPoint.Count,
            overdue,
            todaysFacts.Sum(fact => fact.Attended),
            todaysFacts.Count,
            recent.Where(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo)
                .Sum(fact => fact.Absent));

        return new AttendanceOverviewDto(
            DateOnly.FromDateTime(today),
            kpis,
            toPoint,
            await LoadPointedAsync(cancellationToken),
            await LoadCourseRatesAsync(recent, cancellationToken),
            await LoadToChaseAsync(recentFrom, recentTo, cancellationToken));
    }

    /// <summary>
    /// The month-on-month move, in points. Null unless both months have something
    /// marked in them — "+0 pts" against an empty month would be a claim, not a
    /// measurement.
    /// </summary>
    private static int? AttendanceDelta(
        IReadOnlyCollection<SessionFact> recent,
        IReadOnlyCollection<SessionFact> previous)
    {
        var current = SessionStatistics.AttendanceRate(recent);
        var before = SessionStatistics.AttendanceRate(previous);

        return current is null || before is null ? null : current - before;
    }

    /// <summary>
    /// Sheets still open, over the window <see cref="AttendanceRules.OpenSheets"/>
    /// defines — the same rows the Accueil counts for its alert and its badge.
    /// </summary>
    private async Task<IReadOnlyList<AttendanceSessionDto>> LoadToPointAsync(
        DateTime now,
        CancellationToken cancellationToken) =>
        await AttendanceRules.OpenSheets(_dbContext, now)
            .OrderBy(session => session.StartsAt)
            .Select(Projection())
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AttendanceSessionDto>> LoadPointedAsync(
        CancellationToken cancellationToken) =>
        await SessionsQuery()
            .Where(session => session.AttendanceClosedAt != null)
            .OrderByDescending(session => session.StartsAt)
            .Take(RecentSheetCount)
            .Select(Projection())
            .ToListAsync(cancellationToken);

    /// <summary>
    /// A cancelled session has no sheet, so it belongs in neither list — the same
    /// exclusion <see cref="SessionStatistics.LoadAsync"/> applies to the figures.
    /// </summary>
    private IQueryable<Session> SessionsQuery() =>
        _dbContext.Sessions
            .AsNoTracking()
            .Where(session => session.IsActive && session.Status != SessionStatus.Cancelled);

    private static System.Linq.Expressions.Expression<Func<Session, AttendanceSessionDto>> Projection() =>
        session => new AttendanceSessionDto(
            session.Id,
            session.StartsAt,
            session.EndsAt,
            session.CourseTemplate!.Name,
            session.Coach == null ? null : session.Coach.FirstName,
            session.Coach == null ? null : session.Coach.LastName,
            session.Location!.Name,
            session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted),
            session.Registrations!.Count(seat =>
                seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Present),
            session.Registrations!.Count(seat =>
                seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Late),
            session.Registrations!.Count(seat =>
                seat.IsActive && !seat.IsWaitlisted && seat.Status == AttendanceStatus.Absent),
            session.AttendanceClosedAt);

    /// <summary>
    /// "Taux par cours", best attended first. A course whose sheets nobody
    /// pointed drops out rather than showing a bar at nought.
    /// <para>
    /// Private sessions are left out, as they are from a fill rate: the card
    /// compares classes against one another, and the prototype's own list holds
    /// none. The no-shows on a one-to-one still count towards the KPIs — it is
    /// only this ranking they would distort.
    /// </para>
    /// </summary>
    private async Task<IReadOnlyList<CourseAttendanceDto>> LoadCourseRatesAsync(
        IEnumerable<SessionFact> recent,
        CancellationToken cancellationToken)
    {
        var rates = recent
            .Where(fact => fact.Capacity > 1)
            .GroupBy(fact => fact.CourseTemplateId)
            .Select(group => (CourseTemplateId: group.Key, Rate: SessionStatistics.AttendanceRate(group)))
            .Where(course => course.Rate is not null)
            .ToList();

        if (rates.Count == 0)
        {
            return [];
        }

        var ids = rates.Select(course => course.CourseTemplateId).ToList();
        var names = await _dbContext.CourseTemplates
            .AsNoTracking()
            .Where(template => ids.Contains(template.Id))
            .Select(template => new { template.Id, template.Name })
            .ToDictionaryAsync(template => template.Id, template => template.Name, cancellationToken);

        return
        [
            .. rates
                .Where(course => names.ContainsKey(course.CourseTemplateId))
                .Select(course => new CourseAttendanceDto(
                    course.CourseTemplateId,
                    names[course.CourseTemplateId],
                    course.Rate!.Value))
                .OrderByDescending(course => course.Rate)
                .ThenBy(course => course.CourseName)
        ];
    }

    /// <summary>
    /// The members whose absences are piling up.
    /// <para>
    /// The seats are pulled over and grouped in memory rather than aggregated in
    /// SQL: counting per member and per status inside one group is where this
    /// stops translating on a relational provider, and a month of a single gym is
    /// a few thousand rows.
    /// </para>
    /// </summary>
    private async Task<IReadOnlyList<AbsentMemberDto>> LoadToChaseAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        var seats = await _dbContext.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.IsActive &&
                !registration.IsWaitlisted &&
                registration.Session!.IsActive &&
                registration.Session.Status != SessionStatus.Cancelled &&
                registration.Session.StartsAt >= from &&
                registration.Session.StartsAt < to)
            .Select(registration => new
            {
                registration.MemberId,
                registration.Member!.FirstName,
                registration.Member.LastName,
                registration.Status,
                registration.CheckedInAt
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. seats
                .GroupBy(seat => seat.MemberId)
                .Select(group => new AbsentMemberDto(
                    group.Key,
                    group.First().FirstName,
                    group.First().LastName,
                    group.Count(seat => seat.Status == AttendanceStatus.Absent),
                    group.Count(seat => AttendanceRules.IsMarked(seat.Status)),
                    group.Max(seat => seat.CheckedInAt)))
                .Where(member => member.Missed >= ChaseThreshold)
                .OrderByDescending(member => member.Missed)
                .ThenBy(member => member.Booked)
                .ThenBy(member => member.LastName)
                .Take(ChaseListSize)
        ];
    }
}
