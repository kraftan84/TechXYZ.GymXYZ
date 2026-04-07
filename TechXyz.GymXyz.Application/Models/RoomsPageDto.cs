namespace TechXyz.GymXyz.Application.Models;

public sealed record RoomsPageDto(
    int GymId,
    string GymName,
    List<LocationWithRoomsDto> Locations,
    List<RoomWithLocationDto> Rooms);

public sealed record LocationWithRoomsDto(
    int Id,
    string Name,
    AddressDto Address,
    List<RoomDto> Rooms);

public sealed record RoomDto(
    int Id,
    string Name);

public sealed record RoomWithLocationDto(
    int Id,
    string Name,
    int LocationId,
    string LocationName);
