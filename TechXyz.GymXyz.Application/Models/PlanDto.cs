using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One formule, as every screen that shows the catalogue of offers reads it: the
/// cards on the abonnements page, the plan picker on a member's record, and the
/// "Formules &amp; tarifs" panel of the settings at lot 8.
/// </summary>
public sealed record PlanDto(
    int Id,
    string Name,
    string ShortName,
    decimal Price,
    string Unit,
    PlanKind Kind,
    int? CreditCount,
    int ValidityMonths,
    string BillingLabel,
    string? Description,
    string? Tone,
    bool IsFeatured,
    int Rank,
    int MemberCount)
{
    /// <summary>The price as the card prints it — "49 € / mois".</summary>
    public string PriceLabel =>
        $"{Price.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)} {Unit}".Trim();

    /// <summary>
    /// What one month of this plan is worth, for the MRR. A pack contributes
    /// nothing: it is a single purchase, and smoothing it into a recurring
    /// revenue would have the figure claim money that will not come again.
    /// </summary>
    public decimal MonthlyRevenue => Kind == PlanKind.Recurring && ValidityMonths > 0
        ? Price / ValidityMonths
        : 0m;
}
