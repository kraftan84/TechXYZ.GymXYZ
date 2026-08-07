namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// The signed-in person as the shell shows them. Cascaded once by TenantBoundary
/// so no component has to reach for the authentication state itself.
/// </summary>
public sealed record GymUserInfo(string DisplayName, string? Nickname, string? RoleLabel)
{
    public static readonly GymUserInfo Anonymous = new("Invité", null, null);

    /// <summary>Familiar name for the dashboard greeting, full name otherwise.</summary>
    public string GreetingName => string.IsNullOrWhiteSpace(Nickname) ? DisplayName : Nickname;

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
