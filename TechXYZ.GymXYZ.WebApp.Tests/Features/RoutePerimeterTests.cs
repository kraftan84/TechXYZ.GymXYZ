using System.Text.RegularExpressions;
using Shouldly;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// The perimeter table, made executable. Every routable page is read out of its
/// own source and matched against the access it is supposed to carry.
/// <para>
/// A scan rather than a rendering test because there is no bUnit here and a page
/// carries its rule as an attribute, which is a fact about the file. What this
/// cannot prove is that the attribute is honoured — ASP.NET Core does that, and
/// the browser pass confirms it once per lot. What it does prove is the part
/// that rots: that a page added later states which side of the line it is on,
/// instead of silently inheriting the bare <c>[Authorize]</c> from _Imports and
/// opening the gym's revenue to whoever can sign in.
/// </para>
/// </summary>
public class RoutePerimeterTests
{
    /// <summary>Pages every signed-in person may open, coaches included.</summary>
    private static readonly string[] OpenToEverySignedInUser =
    [
        "Home.razor",
        "Planning.razor",
        "Presences.razor",
        "Members.razor",
        "MemberDetails.razor",
        "Cours.razor",
        "CourseDetails.razor"
    ];

    /// <summary>
    /// Pages about running the gym rather than teaching in it. The policy used to
    /// admit a platform admin as well, so a visit inside a customer could act;
    /// with the impersonation removed it admits the gym's manager alone.
    /// </summary>
    private static readonly string[] ManagerOnly =
    [
        "Abonnements.razor",
        "Reglages.razor",
        "Coachs.razor",
        "CoachDetails.razor",
        "Lieux.razor",
        "LocationDetails.razor",
        "Users.razor"
    ];

    /// <summary>The platform's own screens, reserved for TechXYZ.</summary>
    private static readonly string[] PlatformAdminOnly =
    [
        // Two more stood here — /account/client and its exit, the way in and out
        // of a customer's space. They went with the impersonation, and this is
        // now the only screen a platform admin can open at all.
        "Administration.razor"
    ];

    /// <summary>
    /// Reachable without signing in, by explicit opt-out. The four reset screens
    /// belong here by nature: somebody who has lost their password is, by
    /// definition, somebody who cannot sign in to ask for a new one.
    /// </summary>
    private static readonly string[] Anonymous =
    [
        Path.Combine("Account", "Login.razor"),
        Path.Combine("Account", "AccessDenied.razor"),
        Path.Combine("Account", "ForgotPassword.razor"),
        Path.Combine("Account", "ResetLinkSent.razor"),
        Path.Combine("Account", "ResetPassword.razor"),
        Path.Combine("Account", "ResetPasswordDone.razor"),

        // The space request. Anonymous by nature — its whole purpose is to be
        // filled in by somebody who has no account and is asking for one.
        "SpaceRequestPage.razor",

        "Error.razor",
        "NotFound.razor"
    ];

    private static readonly Regex PageDirective = new(@"^@page\s", RegexOptions.Multiline);

    [Theory]
    [MemberData(nameof(ManagerOnlyPages))]
    public void AManagerOnlyPage_ShouldCarryTheGymManagerPolicy(string relativePath)
    {
        Read(relativePath).ShouldContain(
            "@attribute [Authorize(Policy = GymPolicies.GymManager)]",
            customMessage: $"{relativePath} is about running the gym: a coach who types its URL must be refused.");
    }

    [Theory]
    [MemberData(nameof(PlatformAdminPages))]
    public void APlatformPage_ShouldCarryThePlatformAdminPolicy(string relativePath)
    {
        Read(relativePath).ShouldContain("@attribute [Authorize(Policy = GymPolicies.PlatformAdmin)]");
    }

    [Theory]
    [MemberData(nameof(OpenPages))]
    public void APageOpenToEverybody_ShouldCarryNoPolicy(string relativePath)
    {
        // The other direction, and the one a partitioning lot gets wrong: a coach
        // has to keep the screens they work on.
        Read(relativePath).ShouldNotContain(
            "Policy =",
            customMessage: $"{relativePath} is part of a coach's day and must stay open to them.");
    }

    [Theory]
    [MemberData(nameof(AnonymousPages))]
    public void AnAnonymousPage_ShouldOptOutOfAuthentication(string relativePath)
    {
        // The direction this lot could get wrong. Every page inherits [Authorize]
        // from _Imports, so a reset screen that forgets to opt out sends the one
        // person who cannot sign in to the sign-in screen — and back again.
        Read(relativePath).ShouldContain(
            "@attribute [AllowAnonymous]",
            customMessage: $"{relativePath} is reached by somebody who cannot sign in, and must say so.");
    }

    [Fact]
    public void EveryRoutablePage_ShouldBeAccountedForInThisTable()
    {
        // The canary. A new page inherits the bare [Authorize] from _Imports, so
        // forgetting one is invisible on screen and total in effect — this is
        // what makes it loud instead.
        var declared = OpenToEverySignedInUser
            .Concat(ManagerOnly)
            .Concat(PlatformAdminOnly)
            .Concat(Anonymous)
            .Select(Normalise)
            .ToHashSet();

        var onDisk = RoutablePages().Select(Normalise).ToList();

        onDisk.Except(declared).ShouldBeEmpty(
            "A routable page states which side of the perimeter it is on, in RoutePerimeterTests.");
        declared.Except(onDisk).ShouldBeEmpty(
            "This table names a page that no longer exists.");
    }

    public static TheoryData<string> ManagerOnlyPages() => new(ManagerOnly);

    public static TheoryData<string> PlatformAdminPages() => new(PlatformAdminOnly);

    public static TheoryData<string> OpenPages() => new(OpenToEverySignedInUser);

    public static TheoryData<string> AnonymousPages() => new(Anonymous);

    private static string Read(string relativePath) =>
        RepositoryFiles.ReadWebAppFile("Components", "Pages", relativePath);

    private static IEnumerable<string> RoutablePages()
    {
        var pages = new DirectoryInfo(
            Path.Combine(RepositoryFiles.WebApp().FullName, "Components", "Pages"));

        return pages
            .EnumerateFiles("*.razor", SearchOption.AllDirectories)
            .Where(file => PageDirective.IsMatch(File.ReadAllText(file.FullName)))
            .Select(file => Path.GetRelativePath(pages.FullName, file.FullName));
    }

    private static string Normalise(string path) => path.Replace('\\', '/');
}
