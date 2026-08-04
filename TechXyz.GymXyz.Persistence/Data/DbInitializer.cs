using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXyz.GymXyz.Persistence.Data;

public static class DbInitializer
{
    /// <summary>Demo account password. Development seed only.</summary>
    private const string DemoPassword = "GymXyz!2026";

    public static async Task InitializeAsync(IServiceProvider serviceProvider, GymDbContext dbContext)
    {
        if (dbContext.Tenants.Any())
            return;

        var overrideUser = serviceProvider.GetRequiredService<ICurrentUserOverride>();
        var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();

        using (overrideUser.UseTechnicalUser("DbInitializer"))
        {
            var tenant = CreateGymXyzTenant();
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            // Everything below belongs to that tenant: scope the context so the
            // global filter and the TenantId stamping both line up.
            using (tenantContext.UseTenant(tenant.Id, tenant.Slug))
            {
                await SeedRolesAsync(serviceProvider);
                await SeedManagerAsync(serviceProvider, tenant);
                await SeedGymAsync(dbContext, tenant);
            }
        }
    }

    private static Tenant CreateGymXyzTenant()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            DisplayName = "GymXYZ",
            Baseline = "Salle de sport & coaching",
            MarkKind = TenantMarkKind.Kettlebell,
            WordmarkPrefix = "GYM",
            WordmarkAccent = "XYZ",
            Email = "contact@gymxyz.fr",
            Phone = "04 78 12 34 56",
            Siret = "901 234 567 00018",
            Street = "14 rue de la Villette",
            ZipCode = "69003",
            City = "Lyon 3ᵉ",
            Country = "France",
            Capacity = 180,
            IsSolo = false,
            GymPlan = "GymXYZ Pro",
            PlanPrice = 79m,
            PlanRenewalDate = today.AddMonths(1)
        };
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in GymRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }

    private static async Task SeedManagerAsync(IServiceProvider serviceProvider, Tenant tenant)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "dwayne.johnson@gymxyz.fr";
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var manager = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            TenantId = tenant.Id,
            DisplayName = "Dwayne Johnson",
            Nickname = "The Rock",
            RoleLabel = "Gérant"
        };

        var result = await userManager.CreateAsync(manager, DemoPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not seed the demo manager: {errors}");
        }

        await userManager.AddToRoleAsync(manager, GymRoles.GymManager);
    }

    private static async Task SeedGymAsync(GymDbContext dbContext, Tenant tenant)
    {
        var gym = new Gym("GymXYZ Lyon 3ᵉ");
        tenant.AddGym(gym);

        var mainLocation = new Location("GymXYZ Lyon 3ᵉ")
        {
            Address = new Address
            {
                Street = "14 rue de la Villette",
                ZipCode = "69003",
                City = "Lyon 3ᵉ",
                Country = "France"
            }
        };

        mainLocation.AddRoom(new Room("Studio A"));
        mainLocation.AddRoom(new Room("Studio B"));
        mainLocation.AddRoom(new Room("Studio C"));

        gym.AddLocation(mainLocation);

        gym.AddCoach(new Coach("Nora", "Lemoine")
        {
            Email = "nora.lemoine@gymxyz.fr",
            Phone = "06 41 22 18 07"
        });
        gym.AddCoach(new Coach("Samir", "El Amrani")
        {
            Email = "samir.elamrani@gymxyz.fr",
            Phone = "06 55 70 33 12"
        });
        gym.AddCoach(new Coach("Léa", "Fontaine"));

        gym.AddMember(new Member("Laetitia", "Moriceau")
        {
            Email = "laetitia.moriceau@gymxyz.fr"
        });
        gym.AddMember(new Member("Amina", "Benali")
        {
            Email = "amina.benali@gymxyz.fr"
        });
        gym.AddMember(new Member("Lucas", "Martin")
        {
            Email = "lucas.martin@gymxyz.fr"
        });

        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();
    }
}
