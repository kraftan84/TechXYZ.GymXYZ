using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetRoomsPageQueryHandler : IRequestHandler<GetRoomsPageQuery, RoomsPageDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoomsPageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoomsPageDto?> Handle(GetRoomsPageQuery request, CancellationToken cancellationToken)
    {
        // For now, the first gym is treated as the default gym.
        var gym = await _unitOfWork
            .Repository<Gym, int>()
            .Entities
            .AsNoTracking()
            .Include(candidate => candidate.Locations!)
            .ThenInclude(location => location.Rooms)
            .Include(candidate => candidate.Locations!)
            .ThenInclude(location => location.Address)
            .OrderBy(gym => gym.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (gym is null)
        {
            return null;
        }

        var mappedLocations = gym.Locations?
            .OrderBy(location => location.Name)
            .Select(location => new LocationWithRoomsDto(
                location.Id,
                location.Name,
                location.Address == null
                    ? new AddressDto(string.Empty, string.Empty, string.Empty, string.Empty)
                    : new AddressDto(
                        location.Address.Street,
                        location.Address.ZipCode,
                        location.Address.City,
                        location.Address.Country),
                location.Rooms?
                    .OrderBy(room => room.Name)
                    .Select(room => new RoomDto(room.Id, room.Name))
                    .ToList() ?? []))
            .ToList() ?? [];

        var mappedRooms = mappedLocations
            .SelectMany(location => location.Rooms
                .Select(room => new RoomWithLocationDto(
                    room.Id,
                    room.Name,
                    location.Id,
                    location.Name)))
            .OrderBy(room => room.Name)
            .ToList();

        var page = new RoomsPageDto(
            gym.Id,
            gym.Name,
            mappedLocations,
            mappedRooms);

        return page;
    }
}
