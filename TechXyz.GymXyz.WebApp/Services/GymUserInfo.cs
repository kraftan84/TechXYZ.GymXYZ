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
}
