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
    private readonly ICurrentUserService _currentUser;

    public GetAttendanceOverviewQueryHandler(IGymDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<AttendanceOverviewDto> Handle(
        GetAttendanceOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var today = now.Date;

        // Every figure on this screen is about the same set of sessions. Taken
        // once so the KPIs, the two lists and the chase card cannot describe
        // different gyms to the same person.
        var scope = CoachScope.For(_currentUser);

        var (recentFrom, recentTo) = SessionStatistics.RecentAttendanceWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        var recent = await SessionStatistics.LoadAsync(
            _dbContext, recentFrom, recentTo, cancellationToken, scope);

        // The month before the one on screen, so the KPI can say which way it is
        // going rather than just where it stands.
        var previous = await SessionStatistics.LoadAsync(
            _dbContext,
            recentFrom.AddDays(-SessionStatistics.RecentAttendanceDays),
            recentFrom,
            cancellationToken,
            scope);

        var toPoint = await LoadToPointAsync(now, scope, cancellationToken);

        // Counted off the list itself rather than off the facts. The facts stop
        // at "now" — that is what a rate is averaged over — so an evening class
        // that has not started yet would be missing from the KPI while sitting
        // in the list right underneath it, and the screen would show two answers
        // to the same question.
        var overdue = toPoint.Count(session => session.StartsAt.Date < today);

        // How many people turned up today — which can only count what has
        // already happened, so the facts are the right source.
        var todaysFacts = recent.Where(fact => fact.StartsAt.Date == today).ToList();

        // The denominator beside it — "12 présents sur 3 séances" — covers the
        // whole day instead, the same span AttendanceRules.OpenSheetWindow uses
        // for the list underneath. Counted off the facts it inherited their cut
        // at "now", so at eight in the morning the tile read "0 sur 0 séances"
        // above a list holding a nine o'clock class. Same defect as the one
        // `overdue` above already answers, one tile along.
        var sessionsToday = await SessionsQuery(scope)
            .CountAsync(
                session => session.StartsAt >= today && session.StartsAt < today.AddDays(1),
                cancellationToken);

        var kpis = new AttendanceKpisDto(
            SessionStatistics.AttendanceRate(recent),
            AttendanceDelta(recent, previous),
            toPoint.Count,
            overdue,
            todaysFacts.Sum(fact => fact.Attended),
            sessionsToday,
            recent.Where(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo)
                .Sum(fact => fact.Absent));

        return new AttendanceOverviewDto(
            DateOnly.FromDateTime(today),
            kpis,
            toPoint,
            await LoadPointedAsync(scope, cancellationToken),
            await LoadCourseRatesAsync(recent, cancellationToken),
            await LoadToChaseAsync(recentFrom, recentTo, scope, cancellationToken));
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
        CoachScope scope,
        CancellationToken cancellationToken) =>
        await AttendanceRules.OpenSheets(_dbContext, now, scope)
            .OrderBy(session => session.StartsAt)
            .Select(Projection())
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AttendanceSessionDto>> LoadPointedAsync(
        CoachScope scope,
        CancellationToken cancellationToken) =>
        await SessionsQuery(scope)
            .Where(session => session.AttendanceClosedAt != null)
            .OrderByDescending(session => session.StartsAt)
            .Take(RecentSheetCount)
            .Select(Projection())
            .ToListAsync(cancellationToken);

    /// <summary>
    /// A cancelled session has no sheet, so it belongs in neither list — the same
    /// exclusion <see cref="SessionStatistics.LoadAsync"/> applies to the figures.
    /// <para>
    /// Narrowed separately from <see cref="AttendanceRules.OpenSheets"/>: this is
    /// the "Séances récentes" list, which that window deliberately does not cover.
    /// </para>
    /// </summary>
    private IQueryable<Session> SessionsQuery(CoachScope scope) =>
        scope.Apply(_dbContext.Sessions.AsNoTracking())
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
        CoachScope scope,
        CancellationToken cancellationToken)
    {
        // Narrowed through the seat's session rather than by CoachScope.Apply:
        // the query starts from Registrations, and the card names people a coach
        // has actually had in front of them.
        var coachId = scope.CoachId;
        var restricted = scope.IsRestricted;

        var seats = await _dbContext.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.IsActive &&
                !registration.IsWaitlisted &&
                registration.Session!.IsActive &&
                registration.Session.Status != SessionStatus.Cancelled &&
                registration.Session.StartsAt >= from &&
                registration.Session.StartsAt < to &&
                (!restricted ||
                 (registration.Session.CoachId != null && registration.Session.CoachId == coachId)))
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
