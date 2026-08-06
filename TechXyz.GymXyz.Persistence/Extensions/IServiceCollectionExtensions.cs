using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXyz.GymXyz.Persistence.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext(configuration, environment);
        services.AddScoped<IGymDbContext>(provider => provider.GetRequiredService<GymDbContext>());

        // The accounts side of the same context. Declared in Application,
        // implemented here because Identity types live on this side of the line.
        services.AddScoped<IUserDirectory, UserDirectory>();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var dbConnectionString = configuration.GetConnectionString("GymXyzDb")
                                 ?? throw new InvalidOperationException("Connection string 'GymXyzDb' not found");
        
        services.AddDbContext<GymDbContext>(options =>
        {
            options.UseMySQL(dbConnectionString);

            if (environment.IsDevelopment())
                options.EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
        });
    }

}
