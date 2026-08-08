using System.Text.RegularExpressions;
using Shouldly;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// One rule, said in three places, held to the same number.
/// <para>
/// The hand-off shipped with the fault this prevents: its note under the field
/// promised 12 characters while its own strength gauge rewarded 8. A screen that
/// disagrees with the server teaches the wrong rule, and the person only finds
/// out when a password the gauge called strong is refused on submit.
/// </para>
/// <para>
/// A source scan rather than a rendering test, for the same reason the route
/// perimeter is one: what rots here is not behaviour but agreement between
/// files, and that is a fact about their text.
/// </para>
/// </summary>
public class PasswordRuleTests
{
    private const int MinimumLength = 12;

    [Fact]
    public void TheServer_ShouldRequireTheAdvertisedLength()
    {
        var program = RepositoryFiles.ReadWebAppFile("Program.cs");

        program.ShouldContain(
            $"options.Password.RequiredLength = {MinimumLength};",
            customMessage: "Identity is what actually refuses a password; it has to want what the screen promises.");
    }

    [Fact]
    public void TheServer_ShouldRequireTheThreeKindsOfCharacterTheNoteNames()
    {
        var program = RepositoryFiles.ReadWebAppFile("Program.cs");

        program.ShouldContain("options.Password.RequireUppercase = true;");
        program.ShouldContain("options.Password.RequireLowercase = true;");
        program.ShouldContain("options.Password.RequireDigit = true;");
    }

    [Fact]
    public void TheStrengthGauge_ShouldCountFromTheSameLength()
    {
        var script = RepositoryFiles.ReadWebAppFile("wwwroot", "js", "gx-auth.js");

        var declared = Regex.Match(script, @"MIN_LENGTH\s*=\s*(\d+)");

        declared.Success.ShouldBeTrue("gx-auth.js must declare the length its first segment rewards.");
        declared.Groups[1].Value.ShouldBe(
            MinimumLength.ToString(),
            customMessage: "A gauge that fills before the server is satisfied is a gauge that lies.");
    }

    [Fact]
    public void TheResetScreen_ShouldStateTheRuleItIsHeldTo()
    {
        var screen = RepositoryFiles.ReadWebAppFile(
            "Components", "Pages", "Account", "ResetPassword.razor");

        screen.ShouldContain($"{MinimumLength} caractères minimum");
    }

    [Fact]
    public void TheSeededDemoPassword_ShouldSatisfyTheRuleItIsCreatedUnder()
    {
        // Not pedantry: the demo accounts are created through
        // UserManager.CreateAsync, which runs the validators. Raising the rule
        // without raising this password seeds a development database containing
        // no accounts at all — and the failure reads as a broken seed rather than
        // a password one character short.
        var seed = File.ReadAllText(Path.Combine(
            RepositoryFiles.Root().FullName,
            "TechXyz.GymXyz.Persistence", "Data", "DbInitializer.cs"));

        var password = Regex.Match(seed, @"DemoPassword\s*=\s*""([^""]+)""");

        password.Success.ShouldBeTrue("DbInitializer must declare the demo password.");

        var value = password.Groups[1].Value;

        value.Length.ShouldBeGreaterThanOrEqualTo(MinimumLength);
        value.ShouldMatch("[A-Z]");
        value.ShouldMatch("[a-z]");
        value.ShouldMatch("[0-9]");
    }
}
