using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Lieux;

/// <summary>
/// Labels and tones of the venue catalogue, taken word for word from the
/// prototype. There are no filter chips on this screen — the prototype draws
/// none — so unlike the other sections this class is wordings only.
/// </summary>
public static class LocationFilters
{
    /// <summary>The chip naming the nature of the venue on its record.</summary>
    public static string LabelFor(LocationKind kind) => kind switch
    {
        LocationKind.Outdoor => "Extérieur",
        LocationKind.Home => "À domicile",
        _ => "Salle"
    };

    public static string LabelFor(LocationStatus status) => status switch
    {
        LocationStatus.HighDemand => "Forte demande",
        LocationStatus.OpenAccess => "Accès libre",
        LocationStatus.WeatherDependent => "Météo-dépendant",
        LocationStatus.ByAppointment => "Sur rendez-vous",
        _ => "Disponible"
    };

    public static GxTone ToneFor(LocationStatus status) => status switch
    {
        LocationStatus.Available => GxTone.Success,
        LocationStatus.WeatherDependent => GxTone.Success,
        // Amber, not red: a room everyone wants is good news with a waiting
        // list behind it, not a fault.
        LocationStatus.HighDemand => GxTone.Warning,
        _ => GxTone.Neutral
    };

    /// <summary>
    /// The "t-*" suffix the card head and the mobile tile carry. Unlike the
    /// course catalogue, where the tint is derived from the intensity, a venue
    /// carries its own: nothing about a room implies a colour.
    /// </summary>
    public static string ToneClass(string? tone) => tone switch
    {
        "brand" => "brand",
        "success" => "success",
        "warning" => "warning",
        "danger" => "danger",
        _ => "neutral"
    };

    /// <summary>The venue's icon, falling back to one that suits its kind.</summary>
    public static string IconFor(string? iconKey, LocationKind kind)
    {
        if (!string.IsNullOrWhiteSpace(iconKey))
        {
            return iconKey;
        }

        return kind switch
        {
            LocationKind.Outdoor => GxIconPaths.Tree,
            LocationKind.Home => GxIconPaths.Home,
            _ => GxIconPaths.Grid
        };
    }

    /// <summary>"20 places", or the one-seat venue called by its name.</summary>
    public static string CapacityLabel(int capacity)
        => capacity == 1 ? "Séance privée" : GxFormats.Plural(capacity, "place", "places");

    /// <summary>"65 m²", written the way the prototype writes it.</summary>
    public static string? AreaLabel(decimal? areaSqm)
        => areaSqm is { } area ? $"{area.ToString("0.##", GxFormats.Culture)} m²" : null;

    /// <summary>
    /// The line under the name on the record: what the venue is for, then what
    /// distinguishes it — its surface and floor, its meeting point, or nothing
    /// at all for a session at the member's home.
    /// </summary>
    public static string SummaryFor(LocationDetailsPageDto location)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(location.TypeLabel))
        {
            parts.Add(location.TypeLabel);
        }

        switch (location.Kind)
        {
            case LocationKind.Outdoor:
                if (AddressLine(location.Address) is { } meetingPoint)
                {
                    parts.Add(meetingPoint);
                }

                break;

            case LocationKind.Home:
                parts.Add("séance individuelle");
                break;

            default:
                if (AreaLabel(location.AreaSqm) is { } area)
                {
                    parts.Add(area);
                }

                if (!string.IsNullOrWhiteSpace(location.Floor))
                {
                    parts.Add(location.Floor);
                }

                break;
        }

        return string.Join(" · ", parts);
    }

    /// <summary>
    /// The address on one line. A venue's address is a meeting point rather than
    /// a postal one — the prototype's park carries a street and nothing else —
    /// so the empty parts are dropped instead of leaving commas behind.
    /// </summary>
    public static string? AddressLine(AddressDto? address)
    {
        if (address is null)
        {
            return null;
        }

        var parts = new[] { address.Street, address.ZipCode, address.City }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        return parts.Count == 0 ? null : string.Join(" ", parts);
    }
}
