using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The Accueil in one round trip.
/// <para>
/// Not one SQL statement — three: the week's sessions, where every member
/// stands, and how many sheets are open. What matters is that the screen asks
/// once and gets a set of figures taken at the same moment; a week counted now
/// beside alerts counted a second later would be two answers to one question.
/// </para>
/// <para>
/// None of the three alert figures is computed here. They come from the readers
/// the Abonnements and Présences screens already use, so a number on the Accueil
/// cannot drift from the screen it sends you to.
/// </para>
/// </summary>
public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardQueryHandler(IGymDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var weekStart = PlanningRules.MondayOf(today);

        var from = weekStart.ToDateTime(TimeOnly.MinValue);
        var to = from.AddDays(7);

        // The same week a coach sees on the Planning. Two pages describing "this
        // week" differently is how somebody concludes a session went missing.
        var scope = CoachScope.For(_currentUser);

        // One pass over the week: the seven counts and today's rows come out of
        // the same list, so the strip and the card underneath cannot disagree.
        //
        // Cancelled sessions are left out. « 4 cours » answers how many classes
        // run that day, and a called-off one does not — the same exclusion the
        // attendance figures make.
        var week = await scope.Apply(_dbContext.Sessions.AsNoTracking())
            .Where(session =>
                session.IsActive &&
                session.Status != SessionStatus.Cancelled &&
                session.StartsAt >= from &&
                session.StartsAt < to)
            .OrderBy(session => session.StartsAt)
            .Select(session => new DashboardClassDto(
                session.Id,
                session.StartsAt,
                session.EndsAt,
                session.CourseTemplate!.Name,
                session.Coach == null ? null : session.Coach.FirstName,
                session.Coach == null ? null : session.Coach.LastName,
                session.Location!.Name,
                session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted),
                session.Capacity))
            .ToListAsync(cancellationToken);

        // Counted, not listed: the Accueil says how many, the Présences screen is
        // where they are worked through.
        var sheetsToPoint = await AttendanceRules
            .OpenSheets(_dbContext, now, scope)
            .CountAsync(cancellationToken);

        // A coach is not shown what the club is owed. The query is skipped rather
        // than filtered afterwards: the amount must not reach the browser at all,
        // and there is nothing here to compute for somebody who cannot act on it.
        DashboardAlertsDto alerts;

        if (scope.IsRestricted)
        {
            alerts = new DashboardAlertsDto(0, 0, 0m, sheetsToPoint);
        }
        else
        {
            var horizon = SubscriptionStatusRules.HorizonFrom(today);
            var standing = await SubscriptionStanding.LoadAsync(_dbContext, today, horizon, cancellationToken);

            var late = standing.Where(row => row.Status == SubscriptionStatus.Late).ToList();

            alerts = new DashboardAlertsDto(
                standing.Count(row => row.Status == SubscriptionStatus.ExpiringSoon),
                late.Count,
                late.Sum(row => row.Price),
                sheetsToPoint);
        }

        return new DashboardDto(
            today,
            weekStart,
            BuildStrip(week, weekStart, today),
            [.. week.Where(session => DateOnly.FromDateTime(session.StartsAt) == today)],
            alerts,
            // Coaches actually on the week's schedule — the « · 6 coachs » beside
            // the dates. Not the size of the team: a coach on holiday is not
            // running anything this week.
            week.Where(session => session.CoachFullName is not null)
                .Select(session => session.CoachFullName)
                .Distinct()
                .Count());
    }

    /// <summary>
    /// Seven cells, Monday to Sunday. A day with nothing on keeps its cell and
    /// reads nought — the strip is a week, not a list of busy days.
    /// </summary>
    private static IReadOnlyList<DashboardDayDto> BuildStrip(
        IEnumerable<DashboardClassDto> week,
        DateOnly weekStart,
        DateOnly today)
    {
        var counts = week
            .GroupBy(session => DateOnly.FromDateTime(session.StartsAt))
            .ToDictionary(group => group.Key, group => group.Count());

        return
        [
            .. Enumerable.Range(0, 7).Select(offset =>
            {
                var date = weekStart.AddDays(offset);

                return new DashboardDayDto(date, counts.GetValueOrDefault(date), date == today);
            })
        ];
    }
}
