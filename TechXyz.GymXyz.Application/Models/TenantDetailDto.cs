namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One customer as the TechXYZ console's two panels read it: the brand on the
/// left, what they pay TechXYZ on the right.
/// </summary>
public sealed record TenantDetailDto(
    int Id,
    string Slug,
    string DisplayName,
    string ThemeKey,
    string? Baseline,
    string? LogoPath,
    string? LogoDarkPath,
    bool CircleLogo,
    string? WordmarkText,
    string? WordmarkPrefix,
    string? WordmarkAccent,
    bool IsSolo,
    string? City,
    string? AreaLabel,
    string? GymPlan,
    string? PlanDescription,
    decimal? PlanPrice,
    DateOnly? PlanRenewalDate,
    int? PlanMemberCap,
    string? PaymentBrand,
    string? PaymentLast4,
    string? PaymentExpiry,
    int MemberCount,
    IReadOnlyList<InvoiceDto> Invoices)
{
    public static readonly TenantDetailDto None = new(
        0, string.Empty, string.Empty, "techxyz", null, null, null, false,
        null, null, null, false, null, null,
        null, null, null, null, null, null, null, null, 0, []);

    /// <summary>A town, or the area an itinerant coach covers — never both.</summary>
    public string? Where => string.IsNullOrWhiteSpace(AreaLabel) ? City : AreaLabel;

    /// <summary>Null when the plan is uncapped, which reads "illimité".</summary>
    public int? UsagePercent => PlanMemberCap is > 0
        ? Math.Min(100, (int)Math.Round(MemberCount * 100d / PlanMemberCap.Value))
        : null;

    /// <summary>True once the customer has supplied a mark of its own.</summary>
    public bool HasLogo => !string.IsNullOrWhiteSpace(LogoPath);
}
