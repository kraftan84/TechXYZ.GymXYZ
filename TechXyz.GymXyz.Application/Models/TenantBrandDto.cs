using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Everything the shell needs to paint a customer's brand. Screens read tokens,
/// never a colour: this carries only the identity that cannot live in CSS.
/// </summary>
public sealed record TenantBrandDto(
    int Id,
    string Slug,
    string ThemeKey,
    string DisplayName,
    string? Baseline,
    TenantMarkKind MarkKind,
    string? LogoPath,
    string? LogoDarkPath,
    bool CircleLogo,
    string? WordmarkText,
    string? WordmarkPrefix,
    string? WordmarkAccent,
    bool IsSolo)
{
    /// <summary>Town the customer operates in — "6 membres · Lyon 3ᵉ".</summary>
    public string? City { get; init; }

    /// <summary>
    /// Postcode of the gym. Carried here because the school-holiday zone is
    /// derived from it, and the planning banner needs it before anything else
    /// about the customer is loaded.
    /// </summary>
    public string? ZipCode { get; init; }
}
