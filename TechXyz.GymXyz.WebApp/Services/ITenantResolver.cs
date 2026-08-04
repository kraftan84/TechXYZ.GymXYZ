using System.Security.Claims;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Services;

public interface ITenantResolver
{
    /// <summary>
    /// Resolves the customer for the current scope. A signed-in user's claim wins;
    /// otherwise the host prefix is used, with a configured fallback in development.
    /// Returns null when the host matches no active customer.
    /// </summary>
    Task<TenantBrandDto?> ResolveAsync(ClaimsPrincipal? user, CancellationToken cancellationToken = default);

    /// <summary>Slug deduced from the current host, without touching the database.</summary>
    string ResolveSlugFromHost();
}
