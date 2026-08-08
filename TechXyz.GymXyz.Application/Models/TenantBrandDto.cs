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
    /// Zone shown instead of a town for a customer who works on the move
    /// ("Thonon et alentours"). Carried beside <see cref="City"/> because
    /// <see cref="Where"/> has to be answerable from the brand alone — the
    /// poster puts it on a public image, and reaching for the address there
    /// would print the one thing an itinerant coach never publishes.
    /// </summary>
    public string? AreaLabel { get; init; }

    /// <summary>
    /// Where this customer is, in the words they chose. The area wins over the
    /// town, the same precedence <c>TenantDetailDto</c> and
    /// <c>TenantSummaryDto</c> already apply.
    /// </summary>
    public string? Where => string.IsNullOrWhiteSpace(AreaLabel) ? City : AreaLabel;

    /// <summary>
    /// Postcode of the gym. Carried here because the school-holiday zone is
    /// derived from it, and the planning banner needs it before anything else
    /// about the customer is loaded.
    /// </summary>
    public string? ZipCode { get; init; }

    /// <summary>
    /// Space closed for non-payment. Carried on the brand because the login
    /// screen is the one place that has to know, and it resolves the brand
    /// before anybody has authenticated — there is no other object in hand at
    /// that moment.
    /// </summary>
    public bool IsSuspended { get; init; }

    /// <summary>
    /// Whether the customer asked for the school holidays on the planning.
    /// Travels beside the postcode for the same reason: the banner decides what
    /// to draw from both at once, and a second round-trip for a boolean would
    /// leave the holidays flashing on before they were hidden.
    /// </summary>
    public bool ShowSchoolVacations { get; init; } = true;

    /// <summary>
    /// Whether the planning actually marks them: the setting <b>and</b> the
    /// postcode it needs to mean anything.
    /// <para>
    /// A customer with no address has no département, and
    /// <c>SchoolZones.ForPostcode</c> answers zone A for want of anything
    /// better — which would have an itinerant coach around Thonon reading the
    /// holidays of a zone nobody chose. Marking nothing is the truthful answer,
    /// and the settings panel says so rather than leaving it a mystery.
    /// </para>
    /// </summary>
    public bool MarksSchoolVacations =>
        ShowSchoolVacations && !string.IsNullOrWhiteSpace(ZipCode);
}
