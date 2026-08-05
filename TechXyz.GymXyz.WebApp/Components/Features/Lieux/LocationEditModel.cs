using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Features.Lieux;

/// <summary>
/// What the create / edit drawer binds to. Which fields it shows follows the
/// kind: a studio has a surface and a floor, an outdoor spot a meeting point
/// and a fallback, a session at home neither.
/// <para>
/// The coordinates are deliberately absent. They are stored on the entity and
/// seeded for the park, but only the weather service reads them and that ships
/// after lot 8 — a field that feeds nothing yet would tell the user it does.
/// </para>
/// </summary>
public sealed class LocationEditModel
{
    public string Name { get; set; } = string.Empty;
    public LocationKind Kind { get; set; } = LocationKind.Studio;
    public string? TypeLabel { get; set; }
    public string? IconKey { get; set; }
    public string? Tone { get; set; }
    public int Capacity { get; set; } = 20;
    public decimal? AreaSqm { get; set; }
    public string? Floor { get; set; }
    public string? Note { get; set; }
    public bool IsOpenAccess { get; set; }
    public int? SiteId { get; set; }
    public string? Street { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public bool IsWeatherDependent { get; set; }
    public int? FallbackLocationId { get; set; }

    /// <summary>One line per chip, in the order they are drawn.</summary>
    public string Equipment { get; set; } = string.Empty;

    public static LocationEditModel ForCreate() => new();

    public static LocationEditModel From(LocationDetailsPageDto location) => new()
    {
        Name = location.Name,
        Kind = location.Kind,
        TypeLabel = location.TypeLabel,
        IconKey = location.IconKey,
        Tone = location.Tone,
        Capacity = location.Capacity,
        AreaSqm = location.AreaSqm,
        Floor = location.Floor,
        Note = location.Note,
        IsOpenAccess = location.IsOpenAccess,
        SiteId = location.SiteId,
        Street = location.Address?.Street,
        ZipCode = location.Address?.ZipCode,
        City = location.Address?.City,
        IsWeatherDependent = location.IsWeatherDependent,
        FallbackLocationId = location.FallbackLocationId,
        Equipment = string.Join(Environment.NewLine, location.Equipment)
    };

    /// <summary>
    /// The equipment as the command wants it. One line is one chip, blanks are
    /// dropped by the command anyway but are not worth sending.
    /// </summary>
    public IReadOnlyList<string> EquipmentLines => Equipment
        .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    /// <summary>
    /// Brings the fields the current kind cannot carry back to nothing, so an
    /// address typed as an outdoor spot does not survive a switch to "à
    /// domicile" and get saved invisibly.
    /// </summary>
    public void ApplyKind(LocationKind kind)
    {
        Kind = kind;

        if (kind != LocationKind.Studio)
        {
            AreaSqm = null;
            Floor = null;
            IsOpenAccess = false;
            SiteId = null;
        }

        if (kind != LocationKind.Outdoor)
        {
            IsWeatherDependent = false;
            FallbackLocationId = null;
        }

        if (kind == LocationKind.Home)
        {
            Capacity = 1;
            Street = null;
            ZipCode = null;
            City = null;
        }
    }
}
