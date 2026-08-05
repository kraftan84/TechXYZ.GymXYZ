using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

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
            // The day's schedule and the weekly heatmap are read from sessions,
            // which the planning produces at lot 5.
            [],
            LocationOccupancyDto.Empty);
    }
}
