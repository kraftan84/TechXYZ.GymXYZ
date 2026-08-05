namespace TechXyz.GymXyz.Application.Models;

public sealed record SitesPageDto(
    int GymId,
    string GymName,
    List<SiteWithLocationsDto> Sites,
    List<LocationWithSiteDto> Locations);

public sealed record SiteWithLocationsDto(
    int Id,
    string Name,
    AddressDto Address,
    List<LocationOptionDto> Locations);

public sealed record LocationWithSiteDto(
    int Id,
    string Name,
    int SiteId,
    string SiteName);
