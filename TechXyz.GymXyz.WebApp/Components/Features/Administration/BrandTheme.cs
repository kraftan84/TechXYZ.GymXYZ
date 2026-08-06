namespace TechXyz.GymXyz.WebApp.Components.Features.Administration;

/// <summary>
/// A skin a customer can be given, as the picker lists it.
/// <para>
/// This catalogue mirrors the token blocks of <c>wwwroot/css/themes.css</c> and
/// has to be kept beside them: a theme is a block of CSS variables plus a line
/// here, which is the whole cost of adding a brand — no screen changes, no
/// redeployment beyond the stylesheet.
/// </para>
/// <para>
/// It carries no colour. The cards paint themselves by putting
/// <c>data-theme</c> on their own element, so every swatch reads the real tokens
/// of the theme it advertises rather than a hex copy that would drift the first
/// time a ramp is retuned.
/// </para>
/// </summary>
public sealed record BrandTheme(string Key, string Label, string Description)
{
    public static readonly IReadOnlyList<BrandTheme> All =
    [
        new("techxyz", "GymXYZ", "Défaut · base TechXYZ"),
        new("teamtrainers", "Team Trainer's", "Monochrome · barre sombre"),
        new("leyssa", "Leyssa Coaching", "Rosé · barre douce")
    ];

    public static BrandTheme For(string? key) =>
        All.FirstOrDefault(theme => theme.Key == key) ?? All[0];
}
