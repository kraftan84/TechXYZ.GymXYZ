namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One customer as the TechXYZ console lists it: who they are, what they pay,
/// and how many members they carry.
/// </summary>
public sealed record TenantSummaryDto(
    int Id,
    string Slug,
    string DisplayName,
    string ThemeKey,
    string? Baseline,
    string? LogoPath,
    bool CircleLogo,
    string? City,
    string? AreaLabel,
    bool IsSolo,
    string? GymPlan,
    decimal? PlanPrice,
    DateOnly? PlanRenewalDate,
    int? PlanMemberCap,
    int MemberCount)
{
    /// <summary>
    /// Where they operate — a town, or the area an itinerant coach covers. The
    /// product never shows both, and a customer on the move has no address.
    /// </summary>
    public string? Where => string.IsNullOrWhiteSpace(AreaLabel) ? City : AreaLabel;

    /// <summary>Null when the plan is uncapped, which reads "illimité".</summary>
    public int? UsagePercent => PlanMemberCap is > 0
        ? Math.Min(100, (int)Math.Round(MemberCount * 100d / PlanMemberCap.Value))
        : null;
}
