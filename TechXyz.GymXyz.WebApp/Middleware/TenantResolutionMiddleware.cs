using Microsoft.AspNetCore.Components.Endpoints;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Middleware;

/// <summary>
/// Resolves the ambient tenant once per request, before anything renders.
/// </summary>
/// <remarks>
/// The brand used to be resolved from inside <c>App.razor</c> and
/// <c>TenantBoundary</c>, both during the same static render pass. That is a
/// database query issued from a component's <c>OnInitializedAsync</c>, and on a
/// page that also opens a Blazor circuit it raced the end of the response: the
/// request scope was disposed with the query still in flight, which disposed the
/// transient <c>GymDbContext</c> and its connection underneath it. The command
/// then failed <c>CheckState</c> before ever reaching MySQL —
/// <c>Connection must be valid and open</c>, debt-register entry 3. Nobody saw
/// it because the second caller, released by the resolver's gate, re-issued the
/// same query and succeeded; only the discarded render paid for it.
/// <para>
/// Resolved here, the query runs in the request pipeline, where the scope is
/// guaranteed to outlive it. The resolver is scoped and caches per scope, so
/// every component that asks afterwards is served from memory and issues no
/// query at all.
/// </para>
/// </remarks>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        // Only a request that will render a page needs a brand. Static assets and
        // the circuit's own websocket would otherwise each pay for a query.
        if (WillRenderComponent(context))
        {
            await tenantResolver.ResolveAsync(context.User, context.RequestAborted);
        }

        await _next(context);
    }

    /// <summary>
    /// The endpoint, not the <c>Accept</c> header. A re-executed status-code page
    /// and a plain address-bar navigation can both arrive with no <c>Accept</c> at
    /// all, and skipping those is what leaves a component resolving mid-render.
    /// </summary>
    private static bool WillRenderComponent(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<ComponentTypeMetadata>() is not null;
}

public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Must sit after <c>UseAuthentication</c>: the tenant comes from a claim,
    /// and before the principal is built there is nothing to read it from.
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}
