namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What a role actually opens, in the words « Équipe &amp; accès » prints under
/// each name — « Planning &amp; présences », « Administration complète ».
/// <para>
/// Derived from the Identity role rather than stored per account: the hand-off
/// draws three distinct scopes for four people, and a column that only ever
/// holds one of three values is a column that will disagree with the role it was
/// copied from. Widening a role's reach is then one edit here, not a migration
/// over every row.
/// </para>
/// <para>
/// Not to be confused with <c>ApplicationUser.RoleLabel</c>, which is free text
/// the gym writes itself — « Coach senior · co-fondatrice ». That says who
/// somebody is; this says what they can open.
/// </para>
/// </summary>
public static class TeamAccessScopes
{
    public const string Manager = "Administration complète";
    public const string Coach = "Planning, cours & présences";
    public const string Member = "Espace membre";
    public const string PlatformAdmin = "Administration TechXYZ";

    /// <summary>Falls back to the member scope: the narrowest, never the widest.</summary>
    public static string Label(string? role) => role switch
    {
        GymRoleNames.GymManager => Manager,
        GymRoleNames.Coach => Coach,
        GymRoleNames.PlatformAdmin => PlatformAdmin,
        _ => Member
    };

    /// <summary>
    /// The roles a gym can hand out from the settings screen. PlatformAdmin is
    /// absent on purpose — a customer must not be able to grant itself the
    /// run of the platform.
    /// <para>
    /// Member is absent for a different reason: members do not sign in. They are
    /// reached by e-mail, so granting the role would create an account that no
    /// screen is meant to serve. The name survives — invitations carry it, and
    /// <see cref="Label"/> still answers for it — but nothing can hand it out.
    /// </para>
    /// </summary>
    public static readonly string[] Assignable =
    [
        GymRoleNames.GymManager,
        GymRoleNames.Coach
    ];

    public static bool IsAssignable(string? role) => role is not null && Assignable.Contains(role);
}
