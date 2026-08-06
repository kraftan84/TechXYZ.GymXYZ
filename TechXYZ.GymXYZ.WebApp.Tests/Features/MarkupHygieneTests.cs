using System.Text.RegularExpressions;
using Shouldly;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// No screen may hard-code what a theme is allowed to change.
/// <para>
/// <c>01-LOTS.md</c> asks lot 11 for "a test that, for each theme, instantiates
/// the main pages and checks no hard-coded style has appeared". Rendering them
/// would prove nothing: a theme is a set of CSS custom properties, and the
/// Blazor renderer never evaluates CSS — every page would come out identical
/// under all three brands whether or not the styles were themeable.
/// </para>
/// <para>
/// What can actually be checked is the markup, and it is the stronger check:
/// a colour written into a <c>style=</c> attribute survives every theme by
/// construction, so finding it in the source is finding the bug itself rather
/// than a symptom of it. Ten lots of discipline mean this starts from a clean
/// sheet — the point is that it stays one.
/// </para>
/// </summary>
public class MarkupHygieneTests
{
    /// <summary>
    /// Literal colours: hex, and the functional notations. A theme re-points
    /// the accent ramp and the whole neutral scale, so any of these written
    /// into a component is a pixel that will stay GymXYZ blue on a client's
    /// screen.
    /// </summary>
    private static readonly Regex LiteralColour = new(
        @"#[0-9a-fA-F]{3,8}\b|\b(?:rgba?|hsla?|color-mix)\s*\(", RegexOptions.Compiled);

    /// <summary>
    /// Literal type: the scale is a token set (<c>--text-2xs</c> … ), and a
    /// brand may rescale its display font — Leyssa runs at
    /// <c>--display-title-scale 1.28</c>, so a px size does not follow it.
    /// </summary>
    private static readonly Regex LiteralType = new(
        @"font-size\s*:\s*[\d.]+|font-family\s*:\s*(?!var\()", RegexOptions.Compiled);

    /// <summary>
    /// Only the <c>style</c> attribute is scanned, not the whole file: a
    /// <c>class="gx-chip danger"</c> is exactly how a screen is meant to ask
    /// for a colour, and prose in a comment is not markup.
    /// </summary>
    private static readonly Regex StyleAttribute = new(
        @"style\s*=\s*(?<quote>[""'])(?<css>.*?)\k<quote>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    [Fact]
    public void NoComponent_ShouldHardCodeAColourInAStyleAttribute()
    {
        Offenders(LiteralColour).ShouldBeEmpty(
            "A brand colour is written into markup. Themes re-point the accent "
            + "ramp and the neutral scale, so this pixel keeps GymXYZ's palette "
            + "under every client. Use a token (var(--color-…)) or a class.");
    }

    [Fact]
    public void NoComponent_ShouldHardCodeATypeSizeOrFamilyInAStyleAttribute()
    {
        Offenders(LiteralType).ShouldBeEmpty(
            "A type size or family is written into markup. Use the scale "
            + "(var(--text-2xs) … ) or var(--font-display), which a brand may "
            + "rescale — Leyssa runs its display font at 1.28.");
    }

    [Fact]
    public void TheScan_ShouldBeLookingAtSomething()
    {
        // A path that stops resolving would turn both tests above into a pair
        // that passes on an empty set — the classic way a guard dies quietly.
        RepositoryFiles.WebAppFiles("*.razor")
            .Count()
            .ShouldBeGreaterThan(50, "Found almost no .razor files — the scan is looking in the wrong place.");
    }

    private static List<string> Offenders(Regex forbidden)
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryFiles.WebAppFiles("*.razor"))
        {
            var lines = File.ReadAllLines(file.FullName);

            for (var index = 0; index < lines.Length; index++)
            {
                foreach (Match style in StyleAttribute.Matches(lines[index]))
                {
                    var css = style.Groups["css"].Value;

                    // Razor interpolation inside style="" is how a component
                    // passes a computed width or a token name through; what it
                    // resolves to is not knowable here, and the components that
                    // do it read from tokens already.
                    if (css.Contains('@'))
                        continue;

                    if (forbidden.IsMatch(css))
                    {
                        offenders.Add(
                            $"{RepositoryFiles.RelativePath(file)}:{index + 1} → style=\"{css}\"");
                    }
                }
            }
        }

        return offenders;
    }
}
