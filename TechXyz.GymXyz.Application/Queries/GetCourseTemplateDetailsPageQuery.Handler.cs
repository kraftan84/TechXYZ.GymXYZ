using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCourseTemplateDetailsPageQueryHandler
    : IRequestHandler<GetCourseTemplateDetailsPageQuery, CourseTemplateDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetCourseTemplateDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseTemplateDetailsPageDto?> Handle(
        GetCourseTemplateDetailsPageQuery request,
        CancellationToken cancellationToken)
    {
        // Projected into an anonymous shape first: the record carries pieces the
        // database knows nothing about (the empty figures, the empty session
        // list), and composing them in the projection is what fails to translate.
        var template = await _dbContext.CourseTemplates
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.CourseTemplateId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.DisciplineId,
                DisciplineName = candidate.Discipline!.Name,
                DisciplineIconKey = candidate.Discipline.IconKey,
                IconKeyOverride = candidate.IconKey,
                candidate.DurationMinutes,
                candidate.Capacity,
                candidate.DefaultLocationId,
                DefaultLocationName = candidate.DefaultLocation == null ? null : candidate.DefaultLocation.Name,
                candidate.Level,
                candidate.Intensity,
                candidate.Price,
                candidate.Description,
                Coaches = candidate.Coaches!
                    .Where(link => link.IsActive && link.Coach!.IsActive)
                    .OrderBy(link => link.Rank)
                    .Select(link => new CourseTemplateCoachDto(
                        link.Coach!.Id,
                        link.Coach.FirstName,
                        link.Coach.LastName,
                        link.Coach.RoleLabel))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return null;
        }

        var now = DateTime.Now;
        var (trailingFrom, trailingTo) = SessionStatistics.TrailingWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        var facts = (await SessionStatistics.LoadAsync(
                _dbContext, trailingFrom, trailingTo > weekTo ? trailingTo : weekTo, cancellationToken))
            .Where(fact => fact.CourseTemplateId == template.Id)
            .ToList();

        // "Habitués": people who have come back to this course rather than tried
        // it once. Two visits over the window is the line.
        var regulars = facts.Count == 0
            ? (int?)null
            : await _dbContext.Registrations
                .AsNoTracking()
                .Where(seat => seat.IsActive &&
                               seat.Session!.CourseTemplateId == template.Id &&
                               seat.Session.Status != SessionStatus.Cancelled &&
                               seat.Session.StartsAt >= trailingFrom &&
                               seat.Session.StartsAt < trailingTo)
                .GroupBy(seat => seat.MemberId)
                .CountAsync(group => group.Count() >= RegularVisits, cancellationToken);

        var stats = facts.Count == 0
            ? CourseTemplateStatsDto.Empty
            : new CourseTemplateStatsDto(
                facts.Count(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo),
                SessionStatistics.FillRate(
                    facts.Where(fact => fact.StartsAt >= trailingFrom && fact.StartsAt < trailingTo)),
                regulars);

        var nextSessions = await _dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                session.IsActive &&
                session.CourseTemplateId == template.Id &&
                session.Status != SessionStatus.Cancelled &&
                session.StartsAt >= now)
            .OrderBy(session => session.StartsAt)
            .Take(UpcomingShown)
            .Select(session => new
            {
                session.StartsAt,
                LocationName = session.Location!.Name,
                session.Capacity,
                Registered = session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted)
            })
            .ToListAsync(cancellationToken);

        return new CourseTemplateDetailsPageDto(
            template.Id,
            template.Name,
            template.DisciplineId,
            template.DisciplineName,
            template.DisciplineIconKey,
            template.IconKeyOverride,
            template.DurationMinutes,
            template.Capacity,
            template.DefaultLocationId,
            template.DefaultLocationName,
            template.Level,
            template.Intensity,
            template.Price,
            template.Description,
            template.Coaches,
            nextSessions
                .Select(session => new CourseSessionDto(
                    SessionLabels.ShortDay(session.StartsAt),
                    SessionLabels.Time(session.StartsAt),
                    session.LocationName,
                    session.Registered,
                    session.Capacity))
                .ToList(),
            stats);
    }

    /// <summary>How many of the next occurrences the record lists.</summary>
    private const int UpcomingShown = 5;

    /// <summary>Visits over the window that make a member a regular of the course.</summary>
    private const int RegularVisits = 2;
}
