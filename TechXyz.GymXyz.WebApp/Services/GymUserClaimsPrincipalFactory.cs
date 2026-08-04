using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Writes the tenant into the authentication cookie. Signed by the cookie
/// middleware, it becomes the authoritative tenant for the whole session —
/// a Blazor circuit has no host to re-check against.
/// </summary>
public sealed class GymUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly ISender _sender;

    public GymUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options,
        ISender sender)
        : base(userManager, roleManager, options)
    {
        _sender = sender;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.DisplayName));

        if (!string.IsNullOrWhiteSpace(user.Nickname))
            identity.AddClaim(new Claim(GymClaimTypes.Nickname, user.Nickname));

        if (!string.IsNullOrWhiteSpace(user.RoleLabel))
            identity.AddClaim(new Claim(GymClaimTypes.RoleLabel, user.RoleLabel));

        // A PlatformAdmin has no tenant of its own: it picks one from the
        // Administration screen.
        if (user.TenantId is not { } tenantId)
            return identity;

        identity.AddClaim(new Claim(GymClaimTypes.TenantId, tenantId.ToString()));

        var brand = await _sender.Send(new GetTenantBrandByIdQuery(tenantId));
        if (brand is not null)
            identity.AddClaim(new Claim(GymClaimTypes.TenantSlug, brand.Slug));

        return identity;
    }
}
