namespace TechXyz.GymXyz.WebApp.Services;

public sealed class TenantOptions
{
    public const string SectionName = "Tenant";

    /// <summary>Domain the customer slug is a prefix of (teamtrainers.<b>gymxyz.fr</b>).</summary>
    public string RootDomain { get; set; } = "gymxyz.fr";

    /// <summary>Customer served when the host carries no slug (localhost, apex domain).</summary>
    public string DefaultSlug { get; set; } = "gymxyz";
}
