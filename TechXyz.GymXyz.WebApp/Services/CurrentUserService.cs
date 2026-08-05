using System.Security.Claims;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.WebApp.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;
    private ClaimsPrincipal? _circuitUser;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    /// <summary>
    /// The request's principal while there is an HttpContext, the circuit's once
    /// there is not. Every screen that writes runs in the circuit, so the
    /// fallback is the normal path rather than the exception.
    /// </summary>
    private ClaimsPrincipal? User => _http.HttpContext?.User ?? _circuitUser;

    public string? UserName =>
        CurrentUserOverride.Current
        ?? User?.Identity?.Name
        ?? User?.FindFirstValue("preferred_username")
        ?? User?.FindFirstValue(ClaimTypes.Name);

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    /// <summary>
    /// Carries the signed-in user into a Blazor circuit, where there is no
    /// HttpContext to read from. Set once by TenantBoundary.
    /// <para>
    /// The whole principal is carried, not just the name: a handler asking
    /// <see cref="IsInRole"/> inside a circuit would otherwise find no claims and
    /// refuse a manager their own action.
    /// </para>
    /// </summary>
    public void SetCircuitUser(ClaimsPrincipal? user) => _circuitUser = user;
}
