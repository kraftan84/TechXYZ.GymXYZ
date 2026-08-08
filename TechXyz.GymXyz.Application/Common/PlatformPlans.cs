using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What GymXYZ sells, and the only place it is written down.
/// <para>
/// The hand-off asked for "a single source of truth: the platform's
/// <c>Plans</c> table". <b>There is no such table</b> — a customer's plan is four
/// nullable columns on <c>Tenant</c>, filled at provisioning, which is a contract
/// and not a catalogue. The catalogue is a console screen
/// (<c>07-CONSOLE-PLATEFORME.md</c> §8) that has not been built.
/// </para>
/// <para>
/// So it lives here, once, until the console lot promotes it to rows. Inventing
/// that table now would be guessing at a model whose own screen is still
/// unspecified — and the name <c>Plan</c> is already taken by what a gym sells
/// its members, which is a different thing entirely.
/// </para>
/// <para>
/// The member caps follow the console (<b>150 / 600</b>), not the onboarding
/// mock, which drew 80 for Essentiel. Two documents, one product: the commercial
/// copy is the one that was wrong, and copying it here would have shipped a
/// promise the billing screen contradicts.
/// </para>
/// </summary>
public static class PlatformPlans
{
    // Identifiers in English like the rest of the code; the values are the
    // commercial names customers read, so those stay French.
    public const string Essential = "Essentiel";
    public const string Pro = "Pro";
    public const string Custom = "Sur-mesure";

    /// <summary>The plan a form starts on — the one most customers land on.</summary>
    public const string Default = Pro;

    public static IReadOnlyList<PlatformPlanDto> All { get; } =
    [
        new(
            Essential,
            "49 €",
            "/ mois",
            "Coach indépendant ou petite structure, jusqu'à 150 membres.",
            null,
            [
                "Planning & réservations",
                "Fiches membres",
                "Abonnements et relances"
            ]),
        new(
            Pro,
            "129 €",
            "/ mois",
            "Salle avec équipe de coachs, jusqu'à 600 membres.",
            "Le plus demandé",
            [
                "Tout Essentiel",
                "Multi-lieux & multi-coachs",
                "Présences, QR code, statistiques",
                "Marque blanche complète"
            ]),
        new(
            Custom,
            "Sur devis",
            null,
            "Réseau, franchise ou besoin métier spécifique.",
            null,
            [
                "Tout Pro",
                "Développements dédiés",
                "Reprise de vos données",
                "Interlocuteur unique"
            ])
    ];

    public static bool IsKnown(string? name) =>
        All.Any(plan => plan.Name == name);

    /// <summary>The plan by name, falling back to the default rather than throwing.</summary>
    public static PlatformPlanDto Find(string? name) =>
        All.FirstOrDefault(plan => plan.Name == name) ?? All.Single(plan => plan.Name == Default);
}
