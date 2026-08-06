using System.Text.RegularExpressions;
using Shouldly;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// The application fetches nothing from a third-party host at render time.
/// <para>
/// Until lot 11, <c>themes.css</c> opened with an <c>@import</c> from
/// <c>fonts.googleapis.com</c> for Anton and Dancing Script. Those were the
/// only three outgoing requests the whole product made, and they sent every
/// visitor's IP address to Google before the first paint, with no consent
/// asked — the GDPR problem the hand-off flags and the Munich court decision
/// made concrete.
/// </para>
/// <para>
/// The fix was mechanical; keeping it is the hard part. A brand added later
/// with its own display font is exactly the moment somebody pastes the Google
/// snippet back in, because it is what the font's own page hands you. These
/// tests fail that paste.
/// </para>
/// </summary>
public class SelfHostedFontTests
{
    /// <summary>
    /// Host-shaped sources in a stylesheet: <c>@import url(https://…)</c> and
    /// <c>src: url(//…)</c> both reach off-origin. Data URIs and app-relative
    /// paths are what we want and must not match.
    /// </summary>
    private static readonly Regex ExternalUrl = new(
        @"url\(\s*['""]?(?<url>(https?:)?//[^'""\)]+)", RegexOptions.IgnoreCase);

    [Fact]
    public void NoStylesheet_ShouldFetchAnythingFromAnExternalHost()
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryFiles.WebAppFiles("*.css"))
        {
            var stylesheet = StripComments(File.ReadAllText(file.FullName));

            offenders.AddRange(
                ExternalUrl.Matches(stylesheet)
                    .Select(match =>
                        $"{RepositoryFiles.RelativePath(file)} → {match.Groups["url"].Value}"));
        }

        offenders.ShouldBeEmpty(
            "A stylesheet reaches an external host. Self-host the asset under "
            + "wwwroot/css/techxyz/assets/ instead — see that folder's README.");
    }

    [Theory]
    [InlineData("Orbitron", "Orbitron-latin.woff2")]
    [InlineData("Montserrat", "Montserrat-latin.woff2")]
    [InlineData("Anton", "Anton-latin.woff2")]
    [InlineData("Dancing Script", "DancingScript-latin.woff2")]
    public void EveryDisplayFont_ShouldBeDeclaredAgainstAFileThatExists(
        string family, string fileName)
    {
        var fonts = RepositoryFiles.ReadWebAppFile(
            "wwwroot", "css", "techxyz", "tokens", "fonts.css");

        fonts.ShouldContain(
            $"font-family: \"{family}\"",
            customMessage: $"fonts.css declares no @font-face for {family}.");

        fonts.ShouldContain(
            fileName,
            customMessage: $"fonts.css does not point {family} at {fileName}.");

        // A declaration against a missing file fails silently in the browser:
        // the theme falls back to Montserrat and merely looks a bit off.
        var path = Path.Combine(
            RepositoryFiles.WebApp().FullName,
            "wwwroot", "css", "techxyz", "assets", "fonts", fileName);

        File.Exists(path).ShouldBeTrue($"{family} is declared but {fileName} is not in the repository.");

        new FileInfo(path).Length.ShouldBeGreaterThan(
            1024, $"{fileName} is too small to be a real woff2 — a failed download leaves a stub.");
    }

    [Fact]
    public void EveryFontFamilyOfferedByATheme_ShouldBeOneWeSelfHost()
    {
        // The other direction, and the one that catches a new brand: a theme
        // naming a family nobody hosts renders in the fallback and nothing
        // says so out loud.
        var themes = StripComments(
            RepositoryFiles.ReadWebAppFile("wwwroot", "css", "themes.css"));

        var fonts = RepositoryFiles.ReadWebAppFile(
            "wwwroot", "css", "techxyz", "tokens", "fonts.css");

        var declared = Regex
            .Matches(themes, @"--font-(?:display|accent):\s*(?<stack>[^;]+);")
            .Select(match => match.Groups["stack"].Value)
            .SelectMany(stack => stack.Split(','))
            .Select(family => family.Trim().Trim('"', '\''))
            // Generic families and the token indirections are not ours to host.
            .Where(family => !family.StartsWith("var(")
                             && family is not ("sans-serif" or "serif" or "cursive" or "monospace"))
            .Distinct();

        foreach (var family in declared)
        {
            fonts.ShouldContain(
                $"font-family: \"{family}\"",
                customMessage:
                $"themes.css asks for « {family} » but fonts.css hosts no such family. "
                + "Add the woff2 and its @font-face rather than importing it from a CDN.");
        }
    }

    private static string StripComments(string css) =>
        Regex.Replace(css, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
}
