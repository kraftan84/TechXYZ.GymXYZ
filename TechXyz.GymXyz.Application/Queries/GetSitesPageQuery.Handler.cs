using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetSitesPageQueryHandler : IRequestHandler<GetSitesPageQuery, SitesPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetSitesPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SitesPageDto?> Handle(GetSitesPageQuery request, CancellationToken cancellationToken)
    {
        // For now, the first gym is treated as the default gym.
        var gym = await _dbContext.Gyms
            .AsNoTracking()
            .Where(candidate => candidate.IsActive)
            .Include(candidate => candidate.Sites!)
            .ThenInclude(site => site.Locations)
            .Include(candidate => candidate.Sites!)
            .ThenInclude(site => site.Address)
            .OrderBy(gym => gym.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (gym is null)
        {
            return null;
        }

        var mappedSites = gym.Sites?
            .Where(site => site.IsActive)
            .OrderBy(site => site.Name)
            .Select(site => new SiteWithLocationsDto(
                site.Id,
                site.Name,
                site.Address == null
                    ? new AddressDto(string.Empty, string.Empty, string.Empty, string.Empty)
                    : new AddressDto(
                        site.Address.Street,
                        site.Address.ZipCode,
                        site.Address.City,
                        site.Address.Country),
                site.Locations?
                    .Where(location => location.IsActive)
                    .OrderBy(location => location.Name)
                    .Select(location => new LocationOptionDto(location.Id, location.Name))
                    .ToList() ?? []))
            .ToList() ?? [];

        var mappedLocations = mappedSites
            .SelectMany(site => site.Locations
                .Select(location => new LocationWithSiteDto(
                    location.Id,
                    location.Name,
                    site.Id,
                    site.Name)))
            .OrderBy(location => location.Name)
            .ToList();

        var page = new SitesPageDto(
            gym.Id,
            gym.Name,
            mappedSites,
            mappedLocations);

        return page;
    }
}
