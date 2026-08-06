using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One line of « Horaires d'ouverture » — « Lundi – vendredi · 06:30 – 22:00 ».
/// <para>
/// A day <b>range</b> rather than a row per day: that is how the hand-off prints
/// it, how a gym says it out loud, and what its « Ajouter » button implies. Seven
/// fixed rows would force the panel to collapse them back into ranges to print
/// them, and would have no way to say « fermé le dimanche » other than a row
/// that means nothing.
/// </para>
/// </summary>
public class OpeningHours : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int GymSettingsId { get; set; }

    public GymSettings? Settings { get; set; }

    public DayOfWeek DayFrom { get; set; }

    /// <summary>Same as <see cref="DayFrom"/> for a single day ("Samedi").</summary>
    public DayOfWeek DayTo { get; set; }

    public TimeOnly OpensAt { get; set; }

    public TimeOnly ClosesAt { get; set; }

    /// <summary>Display order. The gym decides it; nothing is sorted by day.</summary>
    public int Rank { get; set; }

    public bool IsSingleDay => DayFrom == DayTo;
}
