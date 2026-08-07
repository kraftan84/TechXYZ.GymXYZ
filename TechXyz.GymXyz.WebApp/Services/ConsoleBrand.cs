using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// What the shell wears for a platform admin who has not entered a customer.
/// <para>
/// It is not a tenant and must never be mistaken for one: <see cref="Id"/> is
/// <c>0</c>, the sentinel the ambient tenant already uses for "nobody", and the
/// global query filter compares <c>TenantId == 0</c> against rows that all carry
/// a real id — so it matches nothing. The console is therefore blind to business
/// data by the same mechanism that keeps two customers apart, not by a separate
/// rule that could drift from it.
/// </para>
/// <para>
/// It carries the platform's own wordmark rather than a customer's. This is the
/// one place a TechXYZ mark is right: the admin is looking at the console, not
/// at a white-labelled product. No customer ever sees it.
/// </para>
/// </summary>
public static class ConsoleBrand
{
    /// <summary>The ambient tenant id meaning "no customer chosen".</summary>
    public const int NoTenantId = 0;

    public static readonly TenantBrandDto Instance = new(
        Id: NoTenantId,
        Slug: string.Empty,
        ThemeKey: "techxyz",
        DisplayName: "Console TechXYZ",
        Baseline: "Administration de la plateforme",
        LogoPath: null,
        LogoDarkPath: null,
        CircleLogo: false,
        WordmarkText: null,
        WordmarkPrefix: "Tech",
        WordmarkAccent: "XYZ",
        IsSolo: false);

    /// <summary>
    /// True when this brand is the console rather than a customer. Read by the
    /// shell to decide what the navigation offers and what a business route
    /// answers.
    /// </summary>
    public static bool IsConsole(TenantBrandDto? brand) => brand is { Id: NoTenantId };
}
