using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetWeekPlanningQueryHandler : IRequestHandler<GetWeekPlanningQuery, WeekPlanningDto>
{
    private readonly IGymDbContext _dbContext;

    public GetWeekPlanningQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeekPlanningDto> Handle(GetWeekPlanningQuery request, CancellationToken cancellationToken)
    {
        var weekStart = PlanningRules.MondayOf(request.WeekStart);
        var from = weekStart.ToDateTime(TimeOnly.MinValue);
        var to = from.AddDays(7);

        var query = _dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                session.IsActive &&
                session.StartsAt >= from &&
                session.StartsAt < to);

        if (request.CoachId is { } coachId)
        {
            query = query.Where(session => session.CoachId == coachId);
        }

        if (request.LocationId is { } locationId)
        {
            query = query.Where(session => session.LocationId == locationId);
        }

        // Private is a capacity of one, the same rule the catalogue applies —
        // written over the session so it translates to SQL.
        query = request.Format switch
        {
            CourseFormat.Private => query.Where(session => session.Capacity == 1),
            CourseFormat.Collective => query.Where(session => session.Capacity > 1),
            _ => query
        };

        var sessions = await query
            .OrderBy(session => session.StartsAt)
            .Select(session => new PlanningSessionDto(
                session.Id,
                session.StartsAt,
                session.EndsAt,
                session.CourseTemplate!.Name,
                session.CourseTemplateId,
                session.Coach == null ? null : session.Coach.FirstName,
                session.Coach == null ? null : session.Coach.LastName,
                session.CoachId,
                session.Location!.Name,
                session.LocationId,
                // A seat on the waiting list is not an occupied one, but the day
                // view counts the queue behind a full class.
                session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted),
                session.Registrations!.Count(seat => seat.IsActive && seat.IsWaitlisted),
                session.Capacity,
                session.Status,
                // What unlocks "this one and all the following" in the drawer.
                session.SeriesId != null,
                session.AttendanceClosedAt))
            .ToListAsync(cancellationToken);

        return new WeekPlanningDto(weekStart, sessions);
    }
}
