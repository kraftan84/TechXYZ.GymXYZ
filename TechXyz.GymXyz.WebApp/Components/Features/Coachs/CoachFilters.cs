using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Features.Coachs;

/// <summary>
/// The three filter chips of the coaches grid, and the wording each standing
/// carries. Taken word for word from the prototype.
/// </summary>
public sealed record CoachFilter(CoachStatus? Status, string Label);

public static class CoachFilters
{
    /// <summary>Days of the availability strip, Monday first.</summary>
    public static readonly IReadOnlyList<string> DayInitials = ["L", "M", "M", "J", "V", "S", "D"];

    /// <summary>Same seven days, spelled out — the mobile strip and the drawer.</summary>
    public static readonly IReadOnlyList<string> DayNames = ["Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim"];

    public static readonly IReadOnlyList<CoachFilter> All =
    [
        new(null, "Tous"),
        new(CoachStatus.Available, "Disponibles"),
        new(CoachStatus.Away, "En congé")
    ];

    public static string LabelFor(CoachStatus status) => status switch
    {
        CoachStatus.Available => "Disponible",
        _ => "En congé"
    };

    /// <summary>Status tones carry meaning and are never themed.</summary>
    public static GxTone ToneFor(CoachStatus status) => status switch
    {
        CoachStatus.Available => GxTone.Success,
        _ => GxTone.Neutral
    };
}
