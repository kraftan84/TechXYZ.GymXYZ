namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The role names, as the strings they are stored and checked as.
/// <para>
/// They live here because Application is the one project both ends can see:
/// <c>GymPolicies</c> is in WebApp and <c>GymRoles</c> in Persistence, so
/// neither could be the source. A handler that refuses an action to everyone but
/// a manager has to name the same string the seeding used, and a second literal
/// is a second thing to get wrong.
/// </para>
/// </summary>
public static class GymRoleNames
{
    public const string GymManager = nameof(GymManager);
    public const string Coach = nameof(Coach);
    public const string Member = nameof(Member);
    public const string PlatformAdmin = nameof(PlatformAdmin);
}
