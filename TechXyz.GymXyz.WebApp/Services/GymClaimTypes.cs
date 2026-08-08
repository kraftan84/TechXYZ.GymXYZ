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

    /// <summary>
    /// The <c>Coach</c> row this account is, when it is one. Present for a
    /// manager too if they also coach — it says who they are on the planning,
    /// not what they may open.
    /// <para>
    /// In the cookie rather than looked up per request for the reason the tenant
    /// is: inside a Blazor circuit there is no HttpContext to read from, and
    /// "whose sessions are these" is asked on every render of Présences.
    /// </para>
    /// </summary>
    public const string CoachId = "gymxyz:coach_id";

    // There used to be a third claim here, gymxyz:impersonation, carrying the id
    // of the visit a platform admin had opened inside a customer. It went with
    // the impersonation itself: an admin now inhabits no customer at all, so the
    // tenant claims beside these are never borrowed — an account either owns one
    // or has none.
}
