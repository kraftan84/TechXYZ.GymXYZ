using System.Text.RegularExpressions;
using Shouldly;
using TechXyz.GymXyz.WebApp.Components.Features.Administration;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// The theme picker's catalogue against the stylesheet it advertises.
/// <para>
/// Adding a brand is a token block in themes.css plus a line in
/// <see cref="BrandTheme.All"/>, and the two halves are in different languages
/// in different folders — exactly the pair that drifts. A theme offered in the
/// picker with no block behind it would paint the customer in the default skin
/// and look like a bug in the white label rather than a missing stylesheet.
/// </para>
/// </summary>
public class BrandThemeTests
{
    [Fact]
    public void EveryOfferedTheme_ShouldHaveItsTokenBlockInTheStylesheet()
    {
        var stylesheet = ReadThemesStylesheet();

        foreach (var theme in BrandTheme.All)
        {
            stylesheet.ShouldContain(
                $"[data-theme=\"{theme.Key}\"]",
                customMessage: $"themes.css carries no token block for « {theme.Label} ».");
        }
    }

    [Fact]
    public void EveryThemeInTheStylesheet_ShouldBeOfferedInThePicker()
    {
        // The other direction: a brand nobody can select is a stylesheet block
        // that quietly stopped being reachable.
        //
        // Comments are stripped first: the file's own header explains how to add
        // a client with a [data-theme="x"] example, and reading that as a real
        // declaration would fail this test on prose.
        var stylesheet = Regex.Replace(
            ReadThemesStylesheet(), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

        var declared = Regex
            .Matches(stylesheet, "\\[data-theme=\"(?<key>[a-z0-9-]+)\"\\]")
            .Select(match => match.Groups["key"].Value)
            .Distinct();

        foreach (var key in declared)
        {
            BrandTheme.All.ShouldContain(
                theme => theme.Key == key,
                customMessage: $"themes.css declares « {key} » but the picker does not offer it.");
        }
    }

    [Fact]
    public void For_ShouldFallBackToTheDefaultSkinOnAnUnknownKey()
    {
        // A customer whose theme was retired still has to render something.
        BrandTheme.For("retiré-en-2027").Key.ShouldBe("techxyz");
        BrandTheme.For(null).Key.ShouldBe("techxyz");
    }

    [Theory]
    [InlineData("app.css")]
    [InlineData("mobile.css")]
    public void CalendarAnnotations_ShouldNotReadTheAccentRamp(string stylesheet)
    {
        // A public holiday and a school break are calendar annotations, not brand
        // decoration. While .ferie read the accent ramp it followed the brand:
        // plain grey under Team Trainer's, rose under Leyssa — so in one calendar
        // the holiday sank into the chrome while the school-break pill beside it
        // stayed amber. Both now take the warning ramp, which no theme re-points,
        // and the icon (star vs sun) is what tells them apart.
        var css = Regex.Replace(
            RepositoryFiles.ReadWebAppFile("wwwroot", "css", stylesheet),
            @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

        var offenders = css
            .Split('\n')
            .Select((line, index) => (line, number: index + 1))
            .Where(entry => Regex.IsMatch(entry.line, @"\.(ferie|vac)\b"))
            .Where(entry => entry.line.Contains("--azure-"))
            .Select(entry => $"{stylesheet}:{entry.number} → {entry.line.Trim()}")
            .ToList();

        offenders.ShouldBeEmpty(
            "A calendar annotation reads the accent ramp, so it changes colour with "
            + "the brand. Use the warning ramp — it is shared and never themed.");
    }

    private static string ReadThemesStylesheet() =>
        RepositoryFiles.ReadWebAppFile("wwwroot", "css", "themes.css");
}
