using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TechXyz.GymXyz.Application.Behaviours;

namespace TechXyz.GymXyz.Application.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediator(); 
    }

    private static void AddMediator(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // First in the pipeline: a caller who may not run the command must be
            // turned away before anything is read on their behalf.
            cfg.AddOpenBehavior(typeof(ManagerOnlyBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
