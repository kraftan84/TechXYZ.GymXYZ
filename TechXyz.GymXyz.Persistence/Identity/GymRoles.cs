namespace TechXyz.GymXyz.Persistence.Identity;

public static class GymRoles
{
    public const string GymManager = nameof(GymManager);
    public const string Coach = nameof(Coach);
    public const string Member = nameof(Member);
    public const string PlatformAdmin = nameof(PlatformAdmin);

    public static readonly string[] All = [GymManager, Coach, Member, PlatformAdmin];
}
