using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A venue a session is booked in: a studio, an outdoor spot, or the member's
/// own home. Called <c>Room</c> until lot 4, which none of the last two is.
/// <para>
/// Occupancy, sessions per week, the day's schedule and the weekly heatmap are
/// all counted from the sessions booked here, so none of them is stored.
/// </para>
/// </summary>
public class Location : EntityBase<int>, ITenantScoped
{
    public Location(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }

    public LocationKind Kind { get; set; }

    /// <summary>
    /// The line under the name — "Cours collectifs", "Plein air · bootcamp". Free
    /// text: it describes what the venue is used for, which no referential can
    /// close.
    /// </summary>
    public string? TypeLabel { get; set; }

    /// <summary>Lucide icon key of the card tile.</summary>
    public string? IconKey { get; set; }

    /// <summary>Tone the tile is tinted with — "brand", "success", "danger", "neutral".</summary>
    public string? Tone { get; set; }

    /// <summary>
    /// How many people fit at once. A venue of the <see cref="LocationKind.Home"/>
    /// kind seats one, and the validator holds it to that.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>Floor area in m². Studios only — the park has no surface to state.</summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>Storey as the record writes it — "Rez-de-chaussée", "Sous-sol".</summary>
    public string? Floor { get; set; }

    public string? Note { get; set; }

    /// <summary>
    /// Open to members during opening hours, with no session booked — what the
    /// prototype chips as "Accès libre" on the weights floor.
    /// </summary>
    public bool IsOpenAccess { get; set; }

    /// <summary>
    /// The building this venue sits in, when it sits in one. The park and the
    /// member's home belong to no site, which is why this is optional.
    /// </summary>
    public int? SiteId { get; set; }
    public Site? Site { get; set; }

    /// <summary>
    /// Where to meet, for a venue that is not a room in the gym. Null for a
    /// studio, which is found through its site, and for the home kind, whose
    /// address lives on the member record and changes with every session.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// Whether rain calls the session off. Stored here and rendered as a chip,
    /// and that is the whole of it: the weather call was dropped from the first
    /// version, so this is something the gym records and reads itself rather
    /// than a trigger. Its coordinates went with it — nothing was reading them.
    /// </summary>
    public bool IsWeatherDependent { get; set; }

    /// <summary>Indoor venue the session falls back to when the weather turns.</summary>
    public int? FallbackLocationId { get; set; }
    public Location? FallbackLocation { get; set; }

    public ICollection<LocationEquipment>? Equipment { get; set; }

    public void AddEquipment(string label, int rank)
    {
        Equipment ??= new List<LocationEquipment>();
        Equipment.Add(new LocationEquipment(label) { Rank = rank });
    }
}
