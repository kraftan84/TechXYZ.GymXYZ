namespace TechXyz.GymXyz.WebApp.Services;

public static class GymPolicies
{
    /// <summary>TechXYZ super-admin. Administration is guarded by policy, not by hiding the link.</summary>
    public const string PlatformAdmin = nameof(PlatformAdmin);

    public const string GymManager = nameof(GymManager);
}
