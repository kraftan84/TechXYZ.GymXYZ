using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachDetailsPageQueryHandler
    : IRequestHandler<GetCoachDetailsPageQuery, CoachDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetCoachDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CoachDetailsPageDto?> Handle(GetCoachDetailsPageQuery request, CancellationToken cancellationToken)
    {
        // Projected into an anonymous shape first: the record carries pieces the
        // database knows nothing about (the empty week, the derived standing),
        // and composing them in the projection is what fails to translate.
        var coach = await _dbContext.Coaches
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.CoachId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.RoleLabel,
                candidate.Bio,
                candidate.JoinedOn,
                candidate.AwayUntil,
                HasAccount = candidate.UserId != null,
                Address = candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country),
                Availability = new WeeklyAvailabilityDto(
                    candidate.AvailableOnMonday,
                    candidate.AvailableOnTuesday,
                    candidate.AvailableOnWednesday,
                    candidate.AvailableOnThursday,
                    candidate.AvailableOnFriday,
                    candidate.AvailableOnSaturday,
                    candidate.AvailableOnSunday),
                Disciplines = candidate.Disciplines!
                    .Where(link => link.IsActive && link.Discipline!.IsActive)
                    .OrderBy(link => link.Rank)
                    .Select(link => new DisciplineDto(
                        link.Discipline!.Id,
                        link.Discipline.Name,
                        link.Discipline.IconKey,
                        link.Discipline.Tone))
                    .ToList(),
                Certifications = candidate.Certifications!
                    .Where(certification => certification.IsActive)
                    .OrderBy(certification => certification.Rank)
                    .Select(certification => certification.Label)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (coach is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.Now;
        var (trailingFrom, trailingTo) = SessionStatistics.TrailingWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        var facts = (await SessionStatistics.LoadAsync(
                _dbContext, trailingFrom, trailingTo > weekTo ? trailingTo : weekTo, cancellationToken))
            .Where(fact => fact.CoachId == coach.Id)
            .ToList();

        var fillRate = SessionStatistics.FillRate(
            facts.Where(fact => fact.StartsAt >= trailingFrom && fact.StartsAt < trailingTo));

        var weekSessionIds = facts
            .Where(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo)
            .Select(fact => fact.SessionId)
            .ToList();

        var weekSessions = await _dbContext.Sessions
            .AsNoTracking()
            .Where(session => weekSessionIds.Contains(session.Id))
            .OrderBy(session => session.StartsAt)
            .Select(session => new
            {
                session.StartsAt,
                CourseName = session.CourseTemplate!.Name,
                session.Capacity,
                Registered = session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted)
            })
            .ToListAsync(cancellationToken);

        // Distinct people seen over the window: "membres suivis" is a head
        // count, not a seat count.
        var followedMembers = facts.Count == 0
            ? (int?)null
            : await _dbContext.Registrations
                .AsNoTracking()
                .Where(seat => seat.IsActive && seat.Session!.CoachId == coach.Id &&
                               seat.Session.StartsAt >= trailingFrom && seat.Session.StartsAt < trailingTo)
                .Select(seat => seat.MemberId)
                .Distinct()
                .CountAsync(cancellationToken);

        var stats = facts.Count == 0
            ? CoachStatsDto.Empty
            : new CoachStatsDto(weekSessionIds.Count, fillRate, followedMembers);

        return new CoachDetailsPageDto(
            coach.Id,
            coach.FirstName,
            coach.LastName,
            coach.Email,
            coach.Phone,
            coach.RoleLabel,
            coach.Bio,
            coach.JoinedOn,
            coach.AwayUntil,
            coach.Address,
            coach.Availability,
            coach.Disciplines,
            coach.Certifications,
            weekSessions
                .Select(session => new CoachSessionDto(
                    SessionLabels.ShortDay(session.StartsAt),
                    SessionLabels.Time(session.StartsAt),
                    session.CourseName,
                    session.Registered,
                    session.Capacity))
                .ToList(),
            stats)
        {
            Status = CoachStatusRules.Resolve(coach.AwayUntil, today, fillRate),
            HasAccount = coach.HasAccount
        };
    }
}
