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

        // One context per handler, not one per circuit.
        //
        // A Blazor circuit's scope lives as long as the tab, and EF forbids two
        // operations at once on the same instance. A screen that fires several
        // queries in one render pass therefore raced against itself: "Connection
        // must be valid and open" and "A second operation was started on this
        // context instance" in the development log, and — once the console of
        // lot 9 read five sources per load — a red toast often enough to be the
        // normal outcome.
        //
        // Transient, from a scoped factory: scoped so the factory can still see
        // the circuit's ITenantContext and ICurrentUserService, transient so
        // every handler gets an instance nobody else is using. No handler
        // changes: a handler was already a unit of work — none sends a nested
        // MediatR request, and nothing opens an explicit transaction — so there
        // was never shared change tracking to lose.
        services.AddTransient<IGymDbContext>(provider =>
            provider.GetRequiredService<IDbContextFactory<GymDbContext>>().CreateDbContext());

        // The accounts side of the same context. Declared in Application,
        // implemented here because Identity types live on this side of the line.
        services.AddScoped<IUserDirectory, UserDirectory>();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var dbConnectionString = configuration.GetConnectionString("GymXyzDb")
                                 ?? throw new InvalidOperationException("Connection string 'GymXyzDb' not found");

        void Configure(DbContextOptionsBuilder options)
        {
            options.UseMySQL(dbConnectionString);

            if (environment.IsDevelopment())
                options.EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
        }

        // GymDbContext itself stays scoped: AddEntityFrameworkStores wants it,
        // and the initializer takes it directly at start-up.
        services.AddDbContext<GymDbContext>(Configure);

        // The factory is scoped rather than the default singleton because the
        // context's constructor takes two scoped services — a singleton factory
        // could not resolve them.
        services.AddDbContextFactory<GymDbContext>(Configure, ServiceLifetime.Scoped);
    }

}
