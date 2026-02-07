using System.Security.Claims;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.WebApp.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    private ClaimsPrincipal? User => _http.HttpContext?.User;

    public string? UserName =>
        CurrentUserOverride.Current
        ?? User?.Identity?.Name
        ?? User?.FindFirstValue("preferred_username")
        ?? User?.FindFirstValue(ClaimTypes.Name);
}