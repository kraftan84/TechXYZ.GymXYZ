namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One of the formulas GymXYZ sells, as the onboarding shows it.
/// </summary>
/// <param name="Name">Also the value stored on a demande — there is no id yet.</param>
/// <param name="Price">Written out, "Sur devis" included: not every plan has a number.</param>
/// <param name="Unit">"/ mois", or null when the price is not periodic.</param>
/// <param name="For">Who it is for, member cap included.</param>
/// <param name="Ribbon">Banner on the card, null for the plans that carry none.</param>
/// <param name="Features">What it includes, in the order the card lists them.</param>
public sealed record PlatformPlanDto(
    string Name,
    string Price,
    string? Unit,
    string For,
    string? Ribbon,
    IReadOnlyList<string> Features);
