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
}
