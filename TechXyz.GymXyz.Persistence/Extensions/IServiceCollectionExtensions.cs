using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Repositories;

namespace TechXyz.GymXyz.Persistence.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext(configuration, environment);
        services.AddRepositories();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var dbConnectionString = configuration.GetConnectionString("GymXyzDb")
                                 ?? throw new InvalidOperationException("Connection string 'TeamTacDb' not found");
        
        services.AddDbContext<GymDbContext>(options =>
        {
            options.UseMySQL(dbConnectionString);

            if (environment.IsDevelopment())
                options.EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services
            .AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork))
            .AddTransient(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
    }
}