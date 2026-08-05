using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLocationDetailsPageQueryHandler
    : IRequestHandler<GetLocationDetailsPageQuery, LocationDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetLocationDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocationDetailsPageDto?> Handle(
        GetLocationDetailsPageQuery request,
        CancellationToken cancellationToken)
    {
        // Projected into an anonymous shape first: the record carries pieces the
        // database knows nothing about (the empty schedule, the empty heatmap),
        // and composing them inside the projection is what fails to translate.
        var location = await _dbContext.Locations
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.LocationId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.Kind,
                candidate.TypeLabel,
                candidate.IconKey,
                candidate.Tone,
                candidate.Capacity,
                candidate.AreaSqm,
                candidate.Floor,
                candidate.Note,
                candidate.IsOpenAccess,
                candidate.IsWeatherDependent,
                candidate.SiteId,
                SiteName = candidate.Site == null ? null : candidate.Site.Name,
                candidate.FallbackLocationId,
                FallbackLocationName = candidate.FallbackLocation == null
                    ? null
                    : candidate.FallbackLocation.Name,
                Address = candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country),
                Equipment = candidate.Equipment!
                    .Where(equipment => equipment.IsActive)
                    .OrderBy(equipment => equipment.Rank)
                    .Select(equipment => equipment.Label)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return null;
        }

        var now = DateTime.Now;
        var today = now.Date;

        var schedule = await _dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                session.IsActive &&
                session.LocationId == location.Id &&
                session.StartsAt >= today &&
                session.StartsAt < today.AddDays(1))
            .OrderBy(session => session.StartsAt)
            .Select(session => new
            {
                session.StartsAt,
                CourseName = session.CourseTemplate!.Name,
                CoachFirstName = session.Coach == null ? null : session.Coach.FirstName,
                CoachLastName = session.Coach == null ? null : session.Coach.LastName,
                session.Capacity,
                session.Status,
                Registered = session.Registrations!.Count(seat => seat.IsActive && !seat.IsWaitlisted)
            })
            .ToListAsync(cancellationToken);

        var todaySessions = schedule
            .Where(session => session.Status != SessionStatus.Cancelled)
            .Select(session => new LocationSessionDto(
                SessionLabels.Time(session.StartsAt),
                session.CourseName,
                session.CoachLastName is null
                    ? null
                    : $"{session.CoachFirstName} {session.CoachLastName}".Trim(),
                session.Registered,
                session.Capacity))
            .ToList();

        var (trailingFrom, trailingTo) = SessionStatistics.TrailingWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        var facts = (await SessionStatistics.LoadAsync(
                _dbContext, trailingFrom, trailingTo > weekTo ? trailingTo : weekTo, cancellationToken))
            .Where(fact => fact.LocationId == location.Id)
            .ToList();

        var weekFacts = facts.Where(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo).ToList();

        var occupancy = facts.Count == 0
            ? LocationOccupancyDto.Empty
            : new LocationOccupancyDto(
                SessionStatistics.FillRate(
                    facts.Where(fact => fact.StartsAt >= trailingFrom && fact.StartsAt < trailingTo)),
                weekFacts.Count,
                SessionStatistics.DailyRates(weekFacts));

        return new LocationDetailsPageDto(
            location.Id,
            location.Name,
            location.Kind,
            location.TypeLabel,
            location.IconKey,
            location.Tone,
            location.Capacity,
            location.AreaSqm,
            location.Floor,
            location.Note,
            location.IsOpenAccess,
            location.IsWeatherDependent,
            location.SiteId,
            location.SiteName,
            location.FallbackLocationId,
            location.FallbackLocationName,
            location.Address,
            location.Equipment,
            todaySessions,
            occupancy);
    }
}
