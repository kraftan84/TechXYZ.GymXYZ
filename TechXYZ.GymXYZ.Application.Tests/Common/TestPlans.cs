using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The two natures of formule, as the seed writes them. Tests take these rather
/// than build a plan each: a pack whose <c>CreditCount</c> disagreed with its
/// <c>ValidityMonths</c> would test a plan the product cannot sell.
/// </summary>
internal static class TestPlans
{
    public static Plan Monthly(string name = "Illimité mensuel") => new()
    {
        Name = name,
        ShortName = "Illimité",
        Price = 49m,
        Unit = "€ / mois",
        Kind = PlanKind.Recurring,
        ValidityMonths = 1,
        BillingLabel = "Sans engagement",
        IsFeatured = true
    };

    public static Plan Pack(string name = "Carte 10 séances", int credits = 10) => new()
    {
        Name = name,
        ShortName = "Carte 10",
        Price = 120m,
        Unit = "€ / carte",
        Kind = PlanKind.CreditPack,
        CreditCount = credits,
        ValidityMonths = 4,
        BillingLabel = "Paiement unique",
        Rank = 1
    };
}
