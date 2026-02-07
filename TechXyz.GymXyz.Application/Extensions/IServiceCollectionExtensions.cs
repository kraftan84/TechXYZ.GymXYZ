using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TechXyz.GymXyz.Application.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediator(); 
    }

    private static void AddMediator(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}