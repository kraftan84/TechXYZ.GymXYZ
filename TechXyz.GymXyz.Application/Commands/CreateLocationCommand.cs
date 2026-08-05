using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommand : IRequest<int>
{
    public CreateLocationCommand(
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

    public string Name { get; }

    public LocationKind Kind { get; }

    /// <summary>One seat is what makes a venue a session at the member's home.</summary>
    public int Capacity { get; }

    public string? TypeLabel { get; }
    public string? IconKey { get; }
    public string? Tone { get; }
    public decimal? AreaSqm { get; }
    public string? Floor { get; }
    public string? Note { get; }

    /// <summary>Open during opening hours, with nothing to book.</summary>
    public bool IsOpenAccess { get; }

    /// <summary>The building it sits in. Null for a venue that sits in none.</summary>
    public int? SiteId { get; }

    // Where to meet, for a venue that is not a room in the gym. The home kind
    // sends none of these: that address is on the member record.
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Country { get; }

    /// <summary>Outdoor venues only: whether rain calls the session off.</summary>
    public bool IsWeatherDependent { get; }

    /// <summary>Indoor venue to fall back to when the weather turns.</summary>
    public int? FallbackLocationId { get; }

    /// <summary>What the venue holds, in the order the chips are drawn.</summary>
    public IReadOnlyList<string>? Equipment { get; }
}
