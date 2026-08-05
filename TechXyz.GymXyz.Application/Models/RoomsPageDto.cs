namespace TechXyz.GymXyz.Application.Models;

public sealed record RoomsPageDto(
    int GymId,
    string GymName,
    List<SiteWithRoomsDto> Sites,
    List<RoomWithSiteDto> Rooms);

public sealed record SiteWithRoomsDto(
    int Id,
    string Name,
    AddressDto Address,
    List<RoomDto> Rooms);

public sealed record RoomDto(
    int Id,
    string Name);

public sealed record RoomWithSiteDto(
    int Id,
    string Name,
    int SiteId,
    string SiteName);
