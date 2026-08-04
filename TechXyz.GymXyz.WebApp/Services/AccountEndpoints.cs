using Microsoft.AspNetCore.Identity;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXyz.GymXyz.WebApp.Services;

public static class AccountEndpoints
{
    /// <summary>
    /// Sign-out needs a real HTTP response to clear the cookie, which a Blazor
    /// circuit cannot produce — hence a plain endpoint.
    /// </summary>
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account");

        group.MapPost("/deconnexion", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();

            return TypedResults.LocalRedirect("/account/connexion");
        });

        return endpoints;
    }
}
