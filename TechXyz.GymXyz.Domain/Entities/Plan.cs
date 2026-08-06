using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A formule — what the gym puts on sale. "Illimité mensuel, 49 € / mois",
/// "Carte 10 séances, 120 €".
/// <para>
/// The plan is the offer, not the sale: a member's own copy of it is a
/// <see cref="Subscription"/>, and the figures that matter to them are snapshot
/// there. Editing a price here changes what the next member pays, never what the
/// current ones already bought.
/// </para>
/// </summary>
public class Plan : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name where the column is narrow — "Illimité" for "Illimité mensuel",
    /// "Carte 10" for "Carte 10 séances". The prototype prints the short form on
    /// every attendance sheet and the full one everywhere else, so the two are
    /// stored rather than one being cut down to the other: "Illimité annuel"
    /// truncated on a word boundary would read "Illimité" too, and the sheet
    /// would stop telling the monthly plan from the yearly one.
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>
    /// What the price is per, as the card prints it — "€ / mois", "€ / carte",
    /// "€ / an". Stored rather than derived from <see cref="Kind"/> because the
    /// two do not line up: monthly and yearly are both recurring.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    public PlanKind Kind { get; set; }

    /// <summary>
    /// Entries a pack is sold with. Null for a recurring plan, whose access is
    /// not counted.
    /// </summary>
    public int? CreditCount { get; set; }

    /// <summary>
    /// How long one purchase covers — 1 for a monthly plan, 12 for a yearly one,
    /// 4 for "10 entrées valables 4 mois". Every plan has one: a pack expires
    /// too, which is what lets a subscription read "Échue depuis 4 j".
    /// </summary>
    public int ValidityMonths { get; set; }

    /// <summary>Engagement, as the card prints it — "Sans engagement", "Engagement 12 mois".</summary>
    public string BillingLabel { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Tone the card is tinted with — "brand", "success", "warning", "neutral".</summary>
    public string? Tone { get; set; }

    /// <summary>The one card the grid puts forward, with a brand rule down its side.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Display order of the cards. Stored because the prototype's order is
    /// neither alphabetical nor by price — it is a commercial choice, and the
    /// featured card is not necessarily first.
    /// </summary>
    public int Rank { get; set; }

    public ICollection<Subscription>? Subscriptions { get; set; }

    /// <summary>
    /// Whether one entry of this plan is counted. Reads off <see cref="Kind"/>
    /// so no caller has to remember which of the two spends credits.
    /// </summary>
    public bool IsCredited => Kind == PlanKind.CreditPack;

    /// <summary>
    /// The price as a subscription records it — "49 € / mois". Built here so the
    /// snapshot taken at the sale reads exactly like the card it was bought
    /// from.
    /// </summary>
    public string FormatPriceLabel() =>
        $"{Price.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)} {Unit}".Trim();
}
