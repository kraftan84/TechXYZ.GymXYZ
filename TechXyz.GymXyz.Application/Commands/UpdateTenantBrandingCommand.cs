using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Saves the « Apparence &amp; marque » panel of one customer.
/// <para>
/// Changing <c>ThemeKey</c> repaints that customer's whole application on their
/// next load, with no redeployment: the mechanism has been in place since lot 0
/// and this is the screen that finally exposes it.
/// </para>
/// <para>
/// The logo is <b>not</b> here. Replacing a mark means receiving a file, and
/// nothing in the product uploads one yet — the card says so and its button is
/// disabled, the way lot 6 treated the check-in kiosk.
/// </para>
/// <para>
/// <see cref="IPlatformScoped"/>: it writes a <c>Tenant</c>, which sits above
/// the global filter, on behalf of an admin who inhabits no customer.
/// </para>
/// </summary>
public sealed record UpdateTenantBrandingCommand(
    int TenantId,
    string ThemeKey,
    string DisplayName,
    string? Baseline,
    string? WordmarkText,
    string? WordmarkPrefix,
    string? WordmarkAccent) : IRequest<bool>, IPlatformScoped;
