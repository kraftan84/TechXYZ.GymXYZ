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

        foreach (var member in CreateDemoMembers())
        {
            gym.AddMember(member);
        }

        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// The six members of the design hand-off demo set, same people as on every
    /// other screen. Dates are relative to today so the demo never goes stale;
    /// the subscription windows are what produce the three standings shown in
    /// the prototype (four active, one expiring, one inactive).
    /// </summary>
    private static IEnumerable<Member> CreateDemoMembers()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        yield return CreateMember(
            "Laetitia", "Moriceau", "laetitia.moriceau@gymxyz.fr", "06 12 34 56 78",
            joinedMonthsAgo: 27, today,
            subscriptionStartsInDays: -12, subscriptionEndsInDays: 18, numberOfLessons: 0,
            notes: "Préfère les cours du matin. Vient surtout en début de semaine.");

        yield return CreateMember(
            "Camille", "Durand", "camille.durand@gymxyz.fr", "06 22 11 90 04",
            joinedMonthsAgo: 17, today,
            subscriptionStartsInDays: -25, subscriptionEndsInDays: 5, numberOfLessons: 10);

        yield return CreateMember(
            "Lucas", "Martin", "lucas.martin@gymxyz.fr", "06 80 45 12 33",
            joinedMonthsAgo: 20, today,
            subscriptionStartsInDays: -40, subscriptionEndsInDays: 50, numberOfLessons: 10);

        yield return CreateMember(
            "Amina", "Benali", "amina.benali@gymxyz.fr", "06 14 78 22 09",
            joinedMonthsAgo: 28, today,
            subscriptionStartsInDays: -8, subscriptionEndsInDays: 22, numberOfLessons: 0);

        yield return CreateMember(
            "Théo", "Garnier", "theo.garnier@gymxyz.fr", "06 55 32 87 41",
            joinedMonthsAgo: 21, today,
            subscriptionStartsInDays: -90, subscriptionEndsInDays: -25, numberOfLessons: 10);

        yield return CreateMember(
            "Sarah", "Cohen", "sarah.cohen@gymxyz.fr", "06 71 09 55 18",
            joinedMonthsAgo: 37, today,
            subscriptionStartsInDays: -3, subscriptionEndsInDays: 27, numberOfLessons: 0);
    }

    private static Member CreateMember(
        string firstName,
        string lastName,
        string email,
        string phone,
        int joinedMonthsAgo,
        DateOnly today,
        int subscriptionStartsInDays,
        int subscriptionEndsInDays,
        int numberOfLessons,
        string? notes = null)
    {
        return new Member(firstName, lastName)
        {
            Email = email,
            Phone = phone,
            JoinedOn = today.AddMonths(-joinedMonthsAgo),
            Notes = notes,
            Subscriptions =
            [
                new Subscription
                {
                    StartDate = today.AddDays(subscriptionStartsInDays),
                    EndDate = today.AddDays(subscriptionEndsInDays),
                    NumberOfLessons = numberOfLessons
                }
            ]
        };
    }
}
