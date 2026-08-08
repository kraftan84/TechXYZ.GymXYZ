namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// The signed-in person as the shell shows them. Cascaded once by TenantBoundary
/// so no component has to reach for the authentication state itself.
/// </summary>
public sealed record GymUserInfo(string DisplayName, string? Nickname, string? RoleLabel)
{
    public static readonly GymUserInfo Anonymous = new("Invité", null, null);

    /// <summary>
    /// How the Accueil says hello: the nickname when the account has one, the
    /// first name otherwise.
    /// <para>
    /// Never the full name. « Bonjour Dwayne Johnson » is how a bank writes to
    /// somebody, not how a gym greets the person who runs it — and the greeting
    /// is the one line on the screen addressed to a person rather than about the
    /// business. The topbar keeps <see cref="DisplayName"/>, which is where the
    /// full name belongs: it identifies the account rather than addressing it.
    /// </para>
    /// </summary>
    public string GreetingName =>
        string.IsNullOrWhiteSpace(Nickname) ? FirstName : Nickname;

    /// <summary>
    /// The first word of the full name. Taken apart here rather than carried as
    /// its own claim because the account holds one name and no split — a coach
    /// row has FirstName and LastName, but a manager may have no coach row at
    /// all, and the greeting must work for both.
    /// <para>
    /// A hyphenated first name survives ("Jean-Pierre Martin" → "Jean-Pierre").
    /// </para>
    /// </summary>
    private string FirstName =>
        DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries) is [var first, ..]
            ? first
            : DisplayName;

    /// <summary>
    /// True while a platform admin is inside a customer. Read from the
    /// impersonation claim once, at the boundary, so the shell never has to ask
    /// the authentication state whose data it is drawing.
    /// </summary>
    public bool IsImpersonating { get; init; }

    /// <summary>
    /// Whether this person runs the gym rather than teaches in it. True for a
    /// platform admin as well, matching <c>GymPolicies.GymManager</c>.
    /// <para>
    /// The role, not the <see cref="RoleLabel"/> beside it: that one is free text
    /// the gym writes itself, and Leyssa's owner wrote "Coach" in it while
    /// holding GymManager. Reading the label to decide what to show would hide
    /// her own settings from her.
    /// </para>
    /// </summary>
    public bool IsManager { get; init; }

    public bool IsPlatformAdmin { get; init; }
}
