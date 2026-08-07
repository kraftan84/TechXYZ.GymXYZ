using System.Security.Claims;
using MediatR;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;

namespace TechXyz.GymXyz.WebApp.Services;

public sealed class TenantResolver : ITenantResolver
{
    private readonly ISender _sender;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantOptions _options;

    // The layout and the boundary both ask during the same render pass. Without
    // this gate they hit the scoped DbContext concurrently, which EF forbids —
    // and the answer cannot change within a scope anyway.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _resolvedSlug;
    private TenantBrandDto? _resolvedBrand;

    public TenantResolver(
        ISender sender,
        IHttpContextAccessor httpContextAccessor,
        TenantOptions options)
    {
        _sender = sender;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public async Task<TenantBrandDto?> ResolveAsync(ClaimsPrincipal? user, CancellationToken cancellationToken = default)
    {
        // An authenticated principal carrying no tenant claim belongs to no
        // customer — a platform admin who has not entered one. It must NOT fall
        // through to the host: on localhost, and on the apex domain in
        // production, that hands back DefaultSlug and the admin silently reads
        // a real customer's data with no TenantImpersonation row recording it.
        //
        // The host fallback exists for the login screen, which has to be dressed
        // in a brand before anybody has authenticated. That is the only case it
        // was ever meant to serve, so it is now the only one it does.
        if (user?.Identity?.IsAuthenticated == true && SlugFromClaims(user) is null)
            return null;

        var slug = SlugFromClaims(user) ?? ResolveSlugFromHost();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_resolvedSlug == slug)
                return _resolvedBrand;

            _resolvedBrand = await _sender.Send(new GetTenantBrandQuery(slug), cancellationToken);
            _resolvedSlug = slug;

            return _resolvedBrand;
        }
        finally
        {
            _gate.Release();
        }
    }

    public string ResolveSlugFromHost()
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
            return _options.DefaultSlug;

        // teamtrainers.gymxyz.fr -> teamtrainers
        var rootDomain = _options.RootDomain;
        if (!string.IsNullOrWhiteSpace(rootDomain)
            && host.EndsWith("." + rootDomain, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = host[..^(rootDomain.Length + 1)];

            // Only a single leading label identifies a customer; "www" is not one.
            if (prefix.Length > 0
                && !prefix.Contains('.')
                && !prefix.Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                return prefix.ToLowerInvariant();
            }
        }

        // localhost, an IP or the apex domain: fall back to the configured customer.
        return _options.DefaultSlug;
    }

    private static string? SlugFromClaims(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var slug = user.FindFirstValue(GymClaimTypes.TenantSlug);

        return string.IsNullOrWhiteSpace(slug) ? null : slug;
    }
}
