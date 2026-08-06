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

    private static string ReadThemesStylesheet() =>
        RepositoryFiles.ReadWebAppFile("wwwroot", "css", "themes.css");
}
