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
