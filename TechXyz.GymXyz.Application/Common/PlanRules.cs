using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What a formule has to satisfy, and the refusals in the words the user reads.
/// Spelled out once so the create and the update cannot disagree about what a
/// sellable offer is.
/// </summary>
public static class PlanRules
{
    public const string NameRequired = "Donnez un nom à la formule.";

    public const string PriceRequired = "Le prix doit être supérieur à zéro.";

    public const string ValidityOutOfRange =
        "La durée de couverture doit être comprise entre 1 et 60 mois.";

    public const string CreditCountRequired =
        "Une carte doit contenir au moins une séance.";

    public const string PlanNotFound = "Formule introuvable.";

    public const string KindIsFixed =
        "La nature d'une formule ne se change pas : un abonnement récurrent et une carte "
        + "ne se consomment pas de la même façon. Créez une nouvelle formule.";

    /// <summary>
    /// The engagement wording the card prints, from the two facts that decide it.
    /// A pack is neither engaged nor not — it is bought once.
    /// </summary>
    public static string BillingLabelFor(PlanKind kind, bool hasCommitment, int validityMonths) =>
        kind == PlanKind.CreditPack
            ? "Paiement unique"
            : hasCommitment
                ? $"Engagement {validityMonths} mois"
                : "Sans engagement";

    /// <summary>
    /// The unit the price is printed with — "€ / mois", "€ / an", "€ / carte".
    /// Derived rather than typed: a plan billed every twelve months and labelled
    /// "€ / mois" would misprice every card it appears on.
    /// </summary>
    public static string UnitFor(PlanKind kind, int validityMonths) =>
        kind == PlanKind.CreditPack
            ? "€ / carte"
            : validityMonths switch
            {
                1 => "€ / mois",
                12 => "€ / an",
                _ => $"€ / {validityMonths} mois"
            };
}
