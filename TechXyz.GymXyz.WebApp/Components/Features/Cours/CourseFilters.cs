using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Cours;

/// <summary>
/// The three filter chips of the catalogue. Taken word for word from the
/// prototype, which derives them from the capacity alone.
/// </summary>
public sealed record CourseFilter(CourseFormat? Format, string Label);

public static class CourseFilters
{
    public static readonly IReadOnlyList<CourseFilter> All =
    [
        new(null, "Tous"),
        new(CourseFormat.Collective, "Collectifs"),
        new(CourseFormat.Private, "Privés")
    ];

    public static string LabelFor(CourseLevel level) => level switch
    {
        CourseLevel.Beginner => "Débutant",
        CourseLevel.Intermediate => "Intermédiaire",
        CourseLevel.Custom => "Sur-mesure",
        _ => "Tous niveaux"
    };

    public static string LabelFor(CourseIntensity intensity) => intensity switch
    {
        CourseIntensity.Gentle => "Douce",
        CourseIntensity.Moderate => "Modérée",
        CourseIntensity.High => "Élevée",
        _ => "Privé"
    };

    /// <summary>
    /// What tints the discipline tile and the intensity chip. The desktop rule:
    /// the mobile prototype tints from the icon instead, which mistints Pilates
    /// and Power Cycle, so this one is used on both shells.
    /// </summary>
    public static GxTone ToneFor(CourseIntensity intensity) => intensity switch
    {
        CourseIntensity.Gentle => GxTone.Success,
        CourseIntensity.Moderate => GxTone.Warning,
        CourseIntensity.High => GxTone.Danger,
        _ => GxTone.Neutral
    };

    /// <summary>The "t-*" suffix the tile and hero classes carry.</summary>
    public static string ToneClass(CourseIntensity intensity)
        => ToneFor(intensity).ToString().ToLowerInvariant();

    /// <summary>"60 min", the way the prototype writes a duration.</summary>
    public static string DurationLabel(int minutes) => $"{minutes} min";

    /// <summary>"20 places", or the private lesson called by its name.</summary>
    public static string CapacityLabel(int capacity)
        => capacity == 1 ? "Privé (1)" : GxFormats.Plural(capacity, "place", "places");

    /// <summary>Same value on the record, where the prototype words it the other way round.</summary>
    public static string CapacityDetailLabel(int capacity)
        => capacity == 1 ? "1 (privé)" : GxFormats.Plural(capacity, "place", "places");

    /// <summary>"Inclus" when the course costs nothing beyond the subscription.</summary>
    public static string PriceLabel(decimal? price)
        => price is { } amount ? $"{GxFormats.Amount(amount)} / séance" : "Inclus";

    public static GxTone PriceTone(decimal? price) => price is null ? GxTone.Success : GxTone.Brand;
}
