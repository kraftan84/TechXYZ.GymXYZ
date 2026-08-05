using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, LocationsPageDto>
{
    private readonly IGymDbContext _dbContext;

    public GetLocationsQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocationsPageDto> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var active = _dbContext.Locations
            .AsNoTracking()
            .Where(location => location.IsActive);

        // Counted over the entity, not over the projected records: a predicate
        // written on the DTO passes in memory and fails to translate to SQL.
        var studioCount = await active.CountAsync(
            location => location.Kind == LocationKind.Studio, cancellationToken);
        var outdoorCount = await active.CountAsync(
            location => location.Kind == LocationKind.Outdoor, cancellationToken);
        var homeCount = await active.CountAsync(
            location => location.Kind == LocationKind.Home, cancellationToken);

        // Grouped by nature, then alphabetical. The prototype's own order is the
        // order its mock array happens to be written in, which is no rule.
        //
        // Ordered by an explicit rank rather than by Kind: the context stores
        // every enum as a string, so ordering on the column would sort "Home"
        // before "Studio" and put the member's living room at the top.
        var items = await active
            .OrderBy(location => location.Kind == LocationKind.Studio
                ? 0
                : location.Kind == LocationKind.Outdoor
                    ? 1
                    : 2)
            .ThenBy(location => location.Name)
            .SelectLocationListItemDto()
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;
        var (trailingFrom, trailingTo) = SessionStatistics.TrailingWindow(now);
        var (weekFrom, weekTo) = SessionStatistics.CurrentWeek(now);

        // One window covers both figures: the trailing weeks contain the week in
        // progress, so the slot count is a slice of the same rows.
        var facts = await SessionStatistics.LoadAsync(
            _dbContext, trailingFrom, trailingTo > weekTo ? trailingTo : weekTo, cancellationToken);

        var byLocation = facts.GroupBy(fact => fact.LocationId).ToDictionary(group => group.Key, group => group.ToList());

        var withFigures = items
            .Select(item =>
            {
                if (!byLocation.TryGetValue(item.Id, out var venueFacts))
                {
                    return item;
                }

                return item with
                {
                    OccupancyRate = SessionStatistics.FillRate(
                        venueFacts.Where(fact => fact.StartsAt >= trailingFrom && fact.StartsAt < trailingTo)),
                    SessionsPerWeek = venueFacts
                        .Count(fact => fact.StartsAt >= weekFrom && fact.StartsAt < weekTo)
                };
            })
            .ToList();

        return new LocationsPageDto(withFigures, studioCount, outdoorCount, homeCount);
    }
}
