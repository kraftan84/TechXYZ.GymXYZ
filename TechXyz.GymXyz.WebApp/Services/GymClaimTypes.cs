namespace TechXyz.GymXyz.WebApp.Services;

public static class GymClaimTypes
{
    /// <summary>
    /// Tenant the account belongs to. Signed inside the authentication cookie,
    /// which makes it the authoritative source once a user is signed in — the
    /// host alone would be tamperable from the circuit.
    /// </summary>
    public const string TenantId = "gymxyz:tenant_id";

    public const string TenantSlug = "gymxyz:tenant_slug";

    /// <summary>Familiar name used in the dashboard greeting.</summary>
    public const string Nickname = "gymxyz:nickname";

    /// <summary>Free-text role shown next to the name ("Gérante", "Coach").</summary>
    public const string RoleLabel = "gymxyz:role_label";
}
