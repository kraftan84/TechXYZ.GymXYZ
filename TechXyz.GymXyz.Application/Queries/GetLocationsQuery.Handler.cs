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

        return new LocationsPageDto(items, studioCount, outdoorCount, homeCount);
    }
}
