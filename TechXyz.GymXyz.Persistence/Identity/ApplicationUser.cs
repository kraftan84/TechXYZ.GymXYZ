using Microsoft.AspNetCore.Identity;

namespace TechXyz.GymXyz.Persistence.Identity;

/// <summary>
/// Application account. Lives in Persistence because it derives from an
/// infrastructure type — Domain must stay free of infrastructure dependencies.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Tenant the account belongs to. Null for a PlatformAdmin.</summary>
    public int? TenantId { get; set; }

    /// <summary>Full name shown in the topbar and the mobile "Plus" sheet.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Familiar name used in the dashboard greeting ("Bonjour The Rock").</summary>
    public string? Nickname { get; set; }

    /// <summary>Free-text role shown next to the name ("Gérante", "Coach", "Accueil").</summary>
    public string? RoleLabel { get; set; }

    public DateTime? LastSeenAt { get; set; }
}
