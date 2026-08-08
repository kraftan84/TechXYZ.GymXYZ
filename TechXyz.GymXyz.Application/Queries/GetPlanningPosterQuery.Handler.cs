using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetPlanningPosterQueryHandler
    : IRequestHandler<GetPlanningPosterQuery, PosterWeekDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetPlanningPosterQueryHandler(IGymDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PosterWeekDto> Handle(
        GetPlanningPosterQuery request,
        CancellationToken cancellationToken)
    {
        var weekStart = PlanningRules.MondayOf(request.WeekStart);
        var from = weekStart.ToDateTime(TimeOnly.MinValue);
        var to = from.AddDays(7);

        // Redundant with IManagerOnly on the query, and kept on purpose: the
        // marker refuses a coach before this runs, so today this narrows
        // nothing. It is here so that the day somebody opens the query to
        // another role — a coach publishing their own sessions was a real
        // proposal — the week they get is at least theirs to see, rather than
        // the gym's whole week arriving through a query that stopped guarding
        // it.
        var scope = CoachScope.For(_currentUser);

        var rows = await scope.Apply(_dbContext.Sessions.AsNoTracking())
            .Where(session =>
                session.IsActive &&
                session.Status != SessionStatus.Cancelled &&
                // A capacity of one is a private session — the same rule the
                // catalogue and the planning filter apply. It never goes on a
                // public image: « Coaching perso » is anonymous in the mockup,
                // but the person it is booked for is not.
                session.Capacity > 1 &&
                session.StartsAt >= from &&
                session.StartsAt < to)
            .OrderBy(session => session.StartsAt)
            .Select(session => new
            {
                session.StartsAt,
                session.EndsAt,
                CourseName = session.CourseTemplate!.Name,
                LocationName = session.Location!.Name,
                CoachFirstName = session.Coach == null ? null : session.Coach.FirstName,
                CoachLastName = session.Coach == null ? null : session.Coach.LastName,
                session.Capacity,
                // A seat on the waiting list is not an occupied one, so it does
                // not take a place away from the number the poster advertises.
                Taken = session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted)
            })
            .ToListAsync(cancellationToken);

        // Shaped in memory rather than in the Select: the abbreviated coach name
        // is string work that has no business being asked of the database, and
        // a week is a few dozen rows.
        var sessions = rows
            .Select(row => new PosterSessionDto(
                row.StartsAt,
                (int)(row.EndsAt - row.StartsAt).TotalMinutes,
                row.CourseName,
                row.LocationName,
                ShortCoachName(row.CoachFirstName, row.CoachLastName),
                Math.Max(0, row.Capacity - row.Taken),
                row.Taken >= row.Capacity))
            .ToList();

        return new PosterWeekDto(weekStart, sessions);
    }

    /// <summary>"N. Lemoine", as the planning writes it, or null for an unstaffed slot.</summary>
    private static string? ShortCoachName(string? firstName, string? lastName) =>
        lastName is null
            ? null
            : string.IsNullOrEmpty(firstName)
                ? lastName
                : $"{firstName[0]}. {lastName}";
}
