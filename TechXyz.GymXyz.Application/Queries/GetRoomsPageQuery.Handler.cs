using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetRoomsPageQueryHandler : IRequestHandler<GetRoomsPageQuery, RoomsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetRoomsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomsPageDto?> Handle(GetRoomsPageQuery request, CancellationToken cancellationToken)
    {
        // For now, the first gym is treated as the default gym.
        var gym = await _dbContext.Gyms
            .AsNoTracking()
            .Where(candidate => candidate.IsActive)
            .Include(candidate => candidate.Sites!)
            .ThenInclude(site => site.Rooms)
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
            .Select(site => new SiteWithRoomsDto(
                site.Id,
                site.Name,
                site.Address == null
                    ? new AddressDto(string.Empty, string.Empty, string.Empty, string.Empty)
                    : new AddressDto(
                        site.Address.Street,
                        site.Address.ZipCode,
                        site.Address.City,
                        site.Address.Country),
                site.Rooms?
                    .Where(room => room.IsActive)
                    .OrderBy(room => room.Name)
                    .Select(room => new RoomDto(room.Id, room.Name))
                    .ToList() ?? []))
            .ToList() ?? [];

        var mappedRooms = mappedSites
            .SelectMany(site => site.Rooms
                .Select(room => new RoomWithSiteDto(
                    room.Id,
                    room.Name,
                    site.Id,
                    site.Name)))
            .OrderBy(room => room.Name)
            .ToList();

        var page = new RoomsPageDto(
            gym.Id,
            gym.Name,
            mappedSites,
            mappedRooms);

        return page;
    }
}
