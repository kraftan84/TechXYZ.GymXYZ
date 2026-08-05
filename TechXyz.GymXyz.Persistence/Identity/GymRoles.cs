using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Persistence.Identity;

/// <summary>
/// The roles as Identity seeds them. The strings themselves come from
/// <see cref="GymRoleNames"/>, which Application can also see — a handler
/// checking for a manager and the seeding that creates one must agree.
/// </summary>
public static class GymRoles
{
    public const string GymManager = GymRoleNames.GymManager;
    public const string Coach = GymRoleNames.Coach;
    public const string Member = GymRoleNames.Member;
    public const string PlatformAdmin = GymRoleNames.PlatformAdmin;

    public static readonly string[] All = [GymManager, Coach, Member, PlatformAdmin];
}
