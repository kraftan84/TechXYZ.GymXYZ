using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Components.Features.Administration;

/// <summary>
/// Edit buffer of « Apparence &amp; marque ». Each panel keeps its own, as on
/// Réglages: the two sections are tabs of one screen but they are not one form,
/// and saving the brand must not quietly post a plan somebody was still reading.
/// </summary>
public sealed class BrandingForm
{
    public string ThemeKey { get; set; } = "techxyz";
    public string DisplayName { get; set; } = string.Empty;
    public string? Baseline { get; set; }

    /// <summary>
    /// Whole wordmark, used when the name is not split. GymXYZ is the one
    /// customer that splits it — "GYM" then "XYZ" in the accent colour.
    /// </summary>
    public string? WordmarkText { get; set; }

    public string? WordmarkPrefix { get; set; }
    public string? WordmarkAccent { get; set; }

    /// <summary>
    /// Which of the two shapes the panel is editing. Held rather than derived so
    /// clearing both halves of a split wordmark does not silently flip the form
    /// back to the whole one while somebody is still typing.
    /// </summary>
    public bool SplitWordmark { get; set; }

    public static BrandingForm From(TenantDetailDto customer) => new()
    {
        ThemeKey = customer.ThemeKey,
        DisplayName = customer.DisplayName,
        Baseline = customer.Baseline,
        WordmarkText = customer.WordmarkText,
        WordmarkPrefix = customer.WordmarkPrefix,
        WordmarkAccent = customer.WordmarkAccent,
        SplitWordmark = !string.IsNullOrWhiteSpace(customer.WordmarkPrefix)
                        || !string.IsNullOrWhiteSpace(customer.WordmarkAccent)
    };

    public UpdateTenantBrandingCommand ToCommand(int tenantId) => new(
        tenantId,
        ThemeKey,
        DisplayName,
        Baseline,
        SplitWordmark ? null : WordmarkText,
        SplitWordmark ? WordmarkPrefix : null,
        SplitWordmark ? WordmarkAccent : null);
}

/// <summary>
/// Edit buffer of « Facturation ». The payment method is absent on purpose: it
/// is what a provider reports, not what anybody types here.
/// </summary>
public sealed class PlanForm
{
    public string? GymPlan { get; set; }
    public string? PlanDescription { get; set; }
    public decimal? PlanPrice { get; set; }
    public DateOnly? PlanRenewalDate { get; set; }

    /// <summary>Null is unlimited, which the hero reads as « illimité ».</summary>
    public int? PlanMemberCap { get; set; }

    public static PlanForm From(TenantDetailDto customer) => new()
    {
        GymPlan = customer.GymPlan,
        PlanDescription = customer.PlanDescription,
        PlanPrice = customer.PlanPrice,
        PlanRenewalDate = customer.PlanRenewalDate,
        PlanMemberCap = customer.PlanMemberCap
    };

    public UpdateTenantPlanCommand ToCommand(int tenantId) => new(
        tenantId,
        GymPlan,
        PlanDescription,
        PlanPrice,
        PlanRenewalDate,
        PlanMemberCap);
}

/// <summary>What the « Nouveau client » drawer collects.</summary>
public sealed class NewTenantForm
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ThemeKey { get; set; } = "techxyz";
    public bool IsSolo { get; set; }

    public CreateTenantCommand ToCommand() => new(Name, Slug, ThemeKey, IsSolo);
}
