using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLocationCommand : IRequest<bool>, IManagerOnly
{
    public UpdateLocationCommand(
        int id,
        string name,
        LocationKind kind,
        int capacity,
        string? typeLabel = null,
        string? iconKey = null,
        string? tone = null,
        decimal? areaSqm = null,
        string? floor = null,
        string? note = null,
        bool isOpenAccess = false,
        int? siteId = null,
        string? street = null,
        string? zipCode = null,
        string? city = null,
        string? country = null,
        bool isWeatherDependent = false,
        int? fallbackLocationId = null,
        IReadOnlyList<string>? equipment = null)
    {
        Id = id;
        Name = name;
        Kind = kind;
        Capacity = capacity;
        TypeLabel = typeLabel;
        IconKey = iconKey;
        Tone = tone;
        AreaSqm = areaSqm;
        Floor = floor;
        Note = note;
        IsOpenAccess = isOpenAccess;
        SiteId = siteId;
        Street = street;
        ZipCode = zipCode;
        City = city;
        Country = country;
        IsWeatherDependent = isWeatherDependent;
        FallbackLocationId = fallbackLocationId;
        Equipment = equipment;
    }

    public int Id { get; }
    public string Name { get; }
    public LocationKind Kind { get; }
    public int Capacity { get; }
    public string? TypeLabel { get; }
    public string? IconKey { get; }
    public string? Tone { get; }
    public decimal? AreaSqm { get; }
    public string? Floor { get; }
    public string? Note { get; }
    public bool IsOpenAccess { get; }
    public int? SiteId { get; }
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Country { get; }
    public bool IsWeatherDependent { get; }
    public int? FallbackLocationId { get; }

    /// <summary>Replaces the list wholesale — an absent list empties it.</summary>
    public IReadOnlyList<string>? Equipment { get; }
}
