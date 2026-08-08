using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXYZ.GymXYZ.WebApp.Tests.Services;

public class TenantResolverTests
{
    private static readonly TenantOptions Options = new()
    {
        RootDomain = "gymxyz.fr",
        DefaultSlug = "gymxyz"
    };

    [Fact]
    public void ResolveSlugFromHost_ShouldUseTheSubdomain_ForAKnownHost()
    {
        var resolver = CreateResolver("teamtrainers.gymxyz.fr");

        resolver.ResolveSlugFromHost().ShouldBe("teamtrainers");
    }

    [Fact]
    public void ResolveSlugFromHost_ShouldFallBackToTheDefault_ForLocalhost()
    {
        var resolver = CreateResolver("localhost");

        resolver.ResolveSlugFromHost().ShouldBe("gymxyz");
    }

    [Fact]
    public void ResolveSlugFromHost_ShouldFallBackToTheDefault_ForTheApexDomain()
    {
        var resolver = CreateResolver("gymxyz.fr");

        resolver.ResolveSlugFromHost().ShouldBe("gymxyz");
    }

    [Fact]
    public void ResolveSlugFromHost_ShouldIgnoreWww()
    {
        var resolver = CreateResolver("www.gymxyz.fr");

        resolver.ResolveSlugFromHost().ShouldBe("gymxyz");
    }

    [Fact]
    public void ResolveSlugFromHost_ShouldFallBackToTheDefault_ForAnUnrelatedDomain()
    {
        var resolver = CreateResolver("teamtrainers.exemple.fr");

        resolver.ResolveSlugFromHost().ShouldBe("gymxyz");
    }

    [Fact]
    public async Task ResolveAsync_ShouldPreferTheUserClaim_OverTheHost()
    {
        // The claim is signed inside the authentication cookie; the host is not.
        var sender = new RecordingSender();
        var resolver = CreateResolver("gymxyz.fr", sender);
        var user = CreatePrincipal(("leyssa", GymClaimTypes.TenantSlug));

        await resolver.ResolveAsync(user);

        sender.LastSlug.ShouldBe("leyssa");
    }

    [Fact]
    public async Task ResolveAsync_ShouldUseTheHost_WhenNobodyIsSignedIn()
    {
        var sender = new RecordingSender();
        var resolver = CreateResolver("teamtrainers.gymxyz.fr", sender);

        await resolver.ResolveAsync(new ClaimsPrincipal(new ClaimsIdentity()));

        sender.LastSlug.ShouldBe("teamtrainers");
    }

    [Fact]
    public async Task ResolveAsync_ShouldResolveNothing_ForAnAuthenticatedUserWithNoTenant()
    {
        // A platform admin. Before lot 11 this fell through to the host, which on
        // localhost — and on the apex domain in production — answers DefaultSlug:
        // the admin read a real customer's members and e-mail addresses in
        // silence. That was entry 2 of the debt register, and closing it is now
        // the whole rule rather than half of it — an admin has no sanctioned way
        // into a customer at all since the impersonation was removed.
        var sender = new RecordingSender();
        var resolver = CreateResolver("gymxyz.fr", sender);
        var admin = CreatePrincipal(("Console TechXYZ", ClaimTypes.Name));

        var brand = await resolver.ResolveAsync(admin);

        brand.ShouldBeNull();
        sender.LastSlug.ShouldBeNull("No brand may be queried at all for an admin outside every customer.");
    }

    [Fact]
    public async Task ResolveAsync_ShouldResolveNothing_EvenOnACustomerSubdomain()
    {
        // The host is not a grant. An admin who lands on teamtrainers.gymxyz.fr
        // without entering that customer is still outside it — otherwise the
        // fallback would just move from the default slug to whichever hostname
        // was typed, which is the same hole with a different key.
        var sender = new RecordingSender();
        var resolver = CreateResolver("teamtrainers.gymxyz.fr", sender);

        var brand = await resolver.ResolveAsync(CreatePrincipal(("Console TechXYZ", ClaimTypes.Name)));

        brand.ShouldBeNull();
        sender.LastSlug.ShouldBeNull();
    }

    // A fourth case stood here: an admin carrying borrowed tenant claims had to
    // keep resolving that customer, so the refusal above did not follow them into
    // a visit they were entitled to. There are no borrowed claims any more — an
    // account either owns a tenant or has none — and the rule that a signed claim
    // beats the host is already pinned by ShouldPreferTheUserClaim_OverTheHost.

    private static TenantResolver CreateResolver(string host, ISender? sender = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString(host);

        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        return new TenantResolver(sender ?? new RecordingSender(), accessor, Options);
    }

    private static ClaimsPrincipal CreatePrincipal(params (string Value, string Type)[] claims)
    {
        var identity = new ClaimsIdentity(
            claims.Select(claim => new Claim(claim.Type, claim.Value)),
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }

    private sealed class RecordingSender : ISender
    {
        public string? LastSlug { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetTenantBrandQuery query)
            {
                LastSlug = query.Slug;
            }

            return Task.FromResult<TResponse>(default!);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => Task.FromResult<object?>(null);

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
