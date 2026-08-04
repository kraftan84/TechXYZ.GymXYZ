namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// Ambient tenant for the current request. Resolved from the host name
/// (<c>teamtrainers</c>.gymxyz.fr) with a configuration fallback in development.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Identifier of the tenant every query and write is scoped to.
    /// Returns <c>0</c> when no tenant could be resolved, so the global filter
    /// matches nothing rather than leaking another customer's data.
    /// </summary>
    int Current { get; }

    /// <summary>False when the host could not be mapped to a known tenant.</summary>
    bool IsResolved { get; }

    /// <summary>Slug of the resolved tenant, or null when unresolved.</summary>
    string? Slug { get; }

    /// <summary>
    /// Temporarily scopes the context to another tenant. Used by the database
    /// initializer and by a PlatformAdmin impersonating a customer from the
    /// Administration screen.
    /// </summary>
    IDisposable UseTenant(int tenantId, string? slug = null);
}
