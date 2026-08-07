using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechXyz.GymXyz.Application.Common;
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
                await SeedAccessAsync(serviceProvider, dbContext, tenant);
                await SeedSettingsAsync(dbContext, tenant);
            }

            await SeedInvoicesAsync(dbContext, tenant);

            // The two other customers of the hand-off. They exist so the TechXYZ
            // console has more than one row to show, and so the white-label
            // promise is finally exercised end to end: themes.css has carried
            // their token blocks since lot 0 and nothing ever selected them.
            await SeedOtherCustomersAsync(serviceProvider, dbContext, tenantContext);

            // Last, and outside every UseTenant: the platform admin belongs to no
            // customer. Seeding it inside one would stamp it with that tenant and
            // hand a super-admin a home it must not have.
            await SeedPlatformAdminAsync(serviceProvider);
        }
    }

    private static Tenant CreateGymXyzTenant()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            DisplayName = "GymXYZ",
            Baseline = "Salle de sport & coaching",
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
            PlanDescription = "Engagement annuel · sans frais de mise en service",
            PlanPrice = 79m,
            PlanRenewalDate = today.AddMonths(1),
            // Pro is the uncapped formula: the billing panel reads "illimité" and
            // draws no gauge. The two other customers are capped, so both shapes
            // of that card are exercised by the seed.
            PlanMemberCap = null,
            PaymentBrand = "Visa",
            PaymentLast4 = "4242",
            PaymentExpiry = "08 / 27"
        };
    }

    /// <summary>
    /// The TechXYZ super-admin. Belongs to no customer — <c>TenantId</c> null is
    /// what tells the claims factory to write no tenant claim, which is what
    /// makes the account land on the console rather than inside a gym.
    /// <para>
    /// Without this account nobody at all can reach <c>/administration</c>: the
    /// page has carried its policy since lot 0 and the seed had no holder for it.
    /// </para>
    /// </summary>
    private static async Task SeedPlatformAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "admin@techxyz.fr";
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            TenantId = null,
            DisplayName = "Console TechXYZ",
            RoleLabel = "Super-admin"
        };

        var result = await userManager.CreateAsync(admin, DemoPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not seed the platform admin: {errors}");
        }

        await userManager.AddToRoleAsync(admin, GymRoles.PlatformAdmin);
    }

    /// <summary>
    /// Team Trainer's and Leyssa Coaching, the two other customers the hand-off
    /// describes (<c>GX_THEMES</c>), each with its manager, its brand lockup, its
    /// GymXYZ plan and its invoices.
    /// <para>
    /// Smaller than GymXYZ but no longer thin. Each carries what GymXYZ cannot
    /// demonstrate on its own — a mark with a dark variant, a circular mark, a
    /// capped plan, a customer with an area instead of an address — and, since
    /// lot 11, a real catalogue, timetable and membership, so that every screen
    /// has something on it under all three brands.
    /// </para>
    /// <para>
    /// They stay deliberately unalike. Team Trainer's is a small strength room
    /// with two coaches and full evening classes; Leyssa is one coach, tiny
    /// groups and private sessions, whose busiest course seats six. A theme that
    /// only ever meets one shape of data proves less than one that meets three.
    /// </para>
    /// </summary>
    private static async Task SeedOtherCustomersAsync(
        IServiceProvider serviceProvider,
        GymDbContext dbContext,
        ITenantContext tenantContext)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var teamTrainers = new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            DisplayName = "Team Trainer's",
            Baseline = "Salle de sport",
            // Whole wordmark rather than a split one: the accent trick is GymXYZ's.
            WordmarkText = "TEAM TRAINER'S",
            LogoPath = "/images/themes/teamtrainers-mark.png",
            // The only customer of the product with a light-on-dark variant, and
            // therefore the only place the lockup's theme switch is exercised.
            LogoDarkPath = "/images/themes/teamtrainers-white.png",
            Email = "contact@teamtrainers.fr",
            Phone = "04 50 26 84 17",
            Siret = "812 456 903 00027",
            Street = "289A Route des Blaves",
            ZipCode = "74200",
            City = "Allinges",
            Country = "France",
            Capacity = 120,
            IsSolo = false,
            GymPlan = "GymXYZ Studio",
            PlanDescription = "Mensuel · jusqu'à 150 membres",
            PlanPrice = 49m,
            PlanRenewalDate = today.AddDays(18),
            PlanMemberCap = 150,
            PaymentBrand = "Mastercard",
            PaymentLast4 = "5417",
            PaymentExpiry = "11 / 28"
        };

        var leyssa = new Tenant("Leyssa Coaching", "leyssa", "leyssa")
        {
            DisplayName = "Leyssa Coaching",
            Baseline = "Coach indépendante",
            WordmarkText = "Leyssa Coaching",
            LogoPath = "/images/themes/leyssa-mark.png",
            CircleLogo = true,
            Email = "najate@leyssa-coaching.fr",
            Phone = "06 24 71 08 33",
            Siret = "923 887 145 00014",
            // No street, no postcode: an itinerant coach has an area, not an
            // address, and the whole product reads AreaLabel instead.
            AreaLabel = "Thonon et alentours",
            Country = "France",
            IsSolo = true,
            // Off, and not merely by taste: with no postcode there is no school
            // zone to follow, so the setting is locked in the panel and the
            // planning marks nothing. It is also the customer the setting was
            // asked for — a solo coach training adults, for whom a yellow band
            // across every week of August says nothing.
            ShowSchoolVacations = false,
            GymPlan = "GymXYZ Solo",
            PlanDescription = "Mensuel · jusqu'à 50 membres",
            PlanPrice = 19m,
            PlanRenewalDate = today.AddDays(6),
            PlanMemberCap = 50,
            PaymentBrand = "Visa",
            PaymentLast4 = "8830",
            PaymentExpiry = "03 / 27"
        };

        dbContext.Tenants.AddRange(teamTrainers, leyssa);
        await dbContext.SaveChangesAsync();

        await SeedSmallCustomerAsync(
            serviceProvider, dbContext, tenantContext, teamTrainers,
            managerEmail: "aurelie.siquier@teamtrainers.fr",
            managerName: "Aurélie Siquier",
            managerNickname: "Lily",
            managerRoleLabel: "Gérante",
            gymName: "Team Trainer's Allinges",
            fillGym: FillTeamTrainersGym,
            coachLogins: ["marine.debord@teamtrainers.fr", "najate.amzil@teamtrainers.fr"]);

        await SeedSmallCustomerAsync(
            serviceProvider, dbContext, tenantContext, leyssa,
            managerEmail: "najate.amzil@leyssa-coaching.fr",
            managerName: "Najate Amzil",
            managerNickname: "Naj",
            managerRoleLabel: "Coach",
            gymName: "Leyssa Coaching",
            fillGym: FillLeyssaGym,
            // She is the customer and she runs the sessions: her manager account
            // is the account of the Coach row her planning hangs off.
            managerCoachEmail: "najate@leyssa-coaching.fr");

        await SeedInvoicesAsync(dbContext, teamTrainers);
        await SeedInvoicesAsync(dbContext, leyssa);
    }

    /// <summary>
    /// A client customer: a manager who can sign in, and a gym filled by the
    /// brand's own <paramref name="fillGym"/>.
    /// <para>
    /// Until lot 11 this stopped at the manager and a handful of members with no
    /// subscription, because the console only counts members. That was enough for
    /// the console and not enough for anything else: with no course, no session
    /// and no cover, every business screen of both client brands rendered its
    /// empty state, and a theme review on empty states inspects empty states —
    /// not the tables, gauges, chips and drawers where a hard-coded style would
    /// actually be hiding.
    /// </para>
    /// <para>
    /// <paramref name="coachLogins"/> names the coaches of this customer who sign
    /// in, by the address on their <see cref="Coach"/> row. They hold
    /// <see cref="GymRoles.Coach"/> — the role whose perimeter the screens now
    /// enforce, so a client brand needs one to prove the partitioning holds
    /// somewhere other than GymXYZ.
    /// </para>
    /// <para>
    /// <paramref name="managerCoachEmail"/> links the manager's own account to a
    /// <see cref="Coach"/> row, for a customer whose manager also runs sessions.
    /// It is a separate parameter because the two addresses need not match: an
    /// independent coach signs in as herself and is listed under her business.
    /// </para>
    /// </summary>
    private static async Task SeedSmallCustomerAsync(
        IServiceProvider serviceProvider,
        GymDbContext dbContext,
        ITenantContext tenantContext,
        Tenant tenant,
        string managerEmail,
        string managerName,
        string managerNickname,
        string managerRoleLabel,
        string gymName,
        Action<GymDbContext, Gym> fillGym,
        string? managerCoachEmail = null,
        IReadOnlyList<string>? coachLogins = null)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        async Task<string> CreateAccountAsync(
            string email,
            string displayName,
            string? nickname,
            string roleLabel,
            string role,
            int lastSeenHoursAgo)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = tenant.Id,
                DisplayName = displayName,
                Nickname = nickname,
                RoleLabel = roleLabel,
                LastSeenAt = DateTime.UtcNow.AddHours(-lastSeenHoursAgo)
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(
                    $"Could not seed the account {email} of {tenant.Slug}: {errors}");
            }

            await userManager.AddToRoleAsync(user, role);
            return user.Id;
        }

        using (tenantContext.UseTenant(tenant.Id, tenant.Slug))
        {
            var managerUserId = await CreateAccountAsync(
                managerEmail, managerName, managerNickname, managerRoleLabel,
                GymRoles.GymManager, lastSeenHoursAgo: 9);

            var gym = new Gym(gymName);
            tenant.AddGym(gym);

            fillGym(dbContext, gym);

            await dbContext.SaveChangesAsync();

            // After the save, so the coaches exist and carry an id.
            var coaches = await dbContext.Coaches.ToListAsync();

            if (managerCoachEmail is not null
                && coaches.FirstOrDefault(coach => coach.Email == managerCoachEmail) is { } managerCoach)
            {
                managerCoach.UserId = managerUserId;
            }

            foreach (var (email, hoursAgo) in (coachLogins ?? []).Select((email, index) => (email, 4 + index * 7)))
            {
                if (coaches.FirstOrDefault(coach => coach.Email == email) is not { } coach)
                    continue;

                coach.UserId = await CreateAccountAsync(
                    email, $"{coach.FirstName} {coach.LastName}", nickname: null,
                    coach.RoleLabel ?? "Coach", GymRoles.Coach, hoursAgo);
            }

            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Team Trainer's: a strength-led room in Allinges, above Thonon. Two coaches,
    /// three venues, four courses and eighteen members, evening classes near full.
    /// <para>
    /// Sized against its own plan rather than GymXYZ's — the customer is on
    /// "GymXYZ Studio", capped at 150 members, so eighteen leaves the billing
    /// gauge in the readable middle of its range instead of pinned at either end.
    /// </para>
    /// </summary>
    private static void FillTeamTrainersGym(GymDbContext dbContext, Gym gym)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var site = new Site("Team Trainer's Allinges")
        {
            Address = new Address
            {
                Street = "289A Route des Blaves",
                ZipCode = "74200",
                City = "Allinges",
                Country = "France"
            }
        };

        var mainFloor = Studio(
            "Plateau principal", "Cours collectifs", "grid", "brand", 18, 85m, "Rez-de-chaussée",
            "Grande salle du rez-de-chaussée, où passent tous les formats collectifs du soir.",
            ["Tapis ×18", "Haltères 2–30 kg", "Kettlebells", "Élastiques", "Sono"]);

        var functionalRoom = Studio(
            "Salle fonctionnelle", "Cross-training", "dumbbell", "warning", 14, 55m, "Rez-de-chaussée",
            "Salle dédiée aux circuits et au cross-training. Sol amortissant, matériel au mur.",
            ["Rameurs ×4", "Assault bikes ×2", "Cordes", "Box plyo", "Barres & disques"]);

        var weightsRoom = Studio(
            "Espace musculation", "Musculation · accès libre", "dumbbell", "neutral", 20, 100m, "1ᵉʳ étage",
            "Plateau en accès libre aux heures d'ouverture. Les coachings individuels s'y déroulent aussi.",
            ["Rack squat ×2", "Bancs", "Poulies", "Haltères 2–40 kg", "Tapis de course ×2"],
            isOpenAccess: true);

        site.AddLocation(mainFloor);
        site.AddLocation(functionalRoom);
        site.AddLocation(weightsRoom);
        gym.AddSite(site);

        var disciplines = CreateClientDisciplines(
            ("Cross-training", "dumbbell", "warning"),
            ("Renforcement", "dumbbell", "brand"),
            ("Cardio", "trend", "warning"),
            ("HIIT", "trend", "danger"),
            ("Coaching perso", "user", "neutral"));

        dbContext.Disciplines.AddRange(disciplines.Values);

        var coaches = new Dictionary<string, Coach>();

        var marine = new Coach("Marine", "Debord")
        {
            RoleLabel = "Coach cross-training",
            Email = "marine.debord@teamtrainers.fr",
            Phone = "06 74 12 55 30",
            JoinedOn = today.AddMonths(-41),
            Bio = "Monte les circuits du soir et suit les membres sur la technique. "
                  + "Marine anime le plateau depuis l'ouverture de la salle.",
            AvailableOnMonday = true, AvailableOnTuesday = false, AvailableOnWednesday = true,
            AvailableOnThursday = true, AvailableOnFriday = true, AvailableOnSaturday = true,
            AvailableOnSunday = false
        };
        marine.AddDiscipline(disciplines["Cross-training"], 0);
        marine.AddDiscipline(disciplines["HIIT"], 1);
        marine.AddCertification("BPJEPS AGFF — Haltérophilie & musculation", 0);
        marine.AddCertification("Préparation physique · FFHM", 1);

        // The same person who owns Leyssa Coaching, salaried here — the two
        // customers are ten minutes apart, which is what makes one coach with two
        // hats a real case rather than a demo convenience. Two accounts carry the
        // two hats, because one account holds one tenant and one role.
        var najate = new Coach("Najate", "Amzil")
        {
            RoleLabel = "Coach renforcement & cardio",
            Email = "najate.amzil@teamtrainers.fr",
            Phone = "06 24 71 08 33",
            JoinedOn = today.AddMonths(-16),
            Bio = "Anime les formats du midi et les séances cardio. Najate accompagne "
                  + "surtout les membres qui reprennent après une pause.",
            AvailableOnMonday = true, AvailableOnTuesday = true, AvailableOnWednesday = true,
            AvailableOnThursday = false, AvailableOnFriday = true, AvailableOnSaturday = true,
            AvailableOnSunday = false
        };
        najate.AddDiscipline(disciplines["Renforcement"], 0);
        najate.AddDiscipline(disciplines["Cardio"], 1);
        najate.AddDiscipline(disciplines["Coaching perso"], 2);
        najate.AddCertification("BPJEPS AF — Cours collectifs", 0);
        najate.AddCertification("PSC1 · premiers secours", 1);

        foreach (var coach in new[] { marine, najate })
        {
            gym.AddCoach(coach);
            coaches[coach.FirstName] = coach;
        }

        var plans = CreateClientPlans(
            new Plan
            {
                Name = "Illimité mensuel",
                ShortName = "Illimité",
                Price = 45m,
                Unit = "€ / mois",
                Kind = PlanKind.Recurring,
                ValidityMonths = 1,
                BillingLabel = "Sans engagement",
                Description = "Accès illimité aux cours collectifs et au plateau.",
                Tone = "brand",
                IsFeatured = true,
                Rank = 0
            },
            new Plan
            {
                Name = "Carte 10 séances",
                ShortName = "Carte 10",
                Price = 99m,
                Unit = "€ / carte",
                Kind = PlanKind.CreditPack,
                CreditCount = 10,
                ValidityMonths = 4,
                BillingLabel = "Paiement unique",
                Description = "10 entrées valables 4 mois.",
                Tone = "neutral",
                Rank = 1
            },
            new Plan
            {
                Name = "Illimité annuel",
                ShortName = "Annuel",
                Price = 450m,
                Unit = "€ / an",
                Kind = PlanKind.Recurring,
                ValidityMonths = 12,
                BillingLabel = "Engagement 12 mois",
                Description = "Deux mois offerts sur l'année.",
                Tone = "warning",
                Rank = 2
            });

        dbContext.Plans.AddRange(plans.Values);

        var templates = new[]
        {
            ClientTemplate(
                "Cross Circuit", disciplines["Cross-training"], 60, 14, functionalRoom,
                CourseLevel.AllLevels, CourseIntensity.High, price: null,
                "Circuit en stations, chronométré. Le format qui remplit la salle le soir.",
                [marine]),
            ClientTemplate(
                "Team Strength", disciplines["Renforcement"], 60, 18, mainFloor,
                CourseLevel.Beginner, CourseIntensity.Moderate, price: null,
                "Mouvements de base à charge légère, en groupe. Le cours d'entrée de la salle.",
                [najate, marine]),
            ClientTemplate(
                "Cardio Boost", disciplines["Cardio"], 45, 18, mainFloor,
                CourseLevel.AllLevels, CourseIntensity.High, price: null,
                "Quarante-cinq minutes de cardio rythmé, sans matériel lourd.",
                [najate]),
            ClientTemplate(
                "Coaching duo", disciplines["Coaching perso"], 60, 2, weightsRoom,
                CourseLevel.Custom, CourseIntensity.Private, price: 60m,
                "Séance à deux avec un coach, sur le plateau. Réservable directement.",
                [najate, marine])
        }.ToDictionary(template => template.Name);

        dbContext.CourseTemplates.AddRange(templates.Values);

        // Eighteen members, and deliberately not all in good standing: the
        // members table and the abonnements screen are where status chips live,
        // and a table where every row reads "Actif" shows one chip out of four.
        var members = new List<Member>
        {
            ClientMember("Marion", "Delaunay", "teamtrainers.fr", "06 31 44 20 18", 26, today,
                plans["Illimité mensuel"], -10, 20,
                notes: "Vient surtout le soir, après le travail."),
            ClientMember("Kevin", "Boucher", "teamtrainers.fr", "06 52 09 77 41", 22, today,
                plans["Illimité annuel"], -120, 245),
            ClientMember("Sofia", "Marchetti", "teamtrainers.fr", "06 27 65 13 09", 19, today,
                plans["Carte 10 séances"], -30, 6, creditsRemaining: 2),
            ClientMember("Hugo", "Perrin", "teamtrainers.fr", "06 84 31 52 76", 17, today,
                plans["Illimité mensuel"], -18, 12),
            ClientMember("Anaïs", "Leroy", "teamtrainers.fr", "06 60 18 94 23", 15, today,
                plans["Illimité mensuel"], -5, 25),
            ClientMember("Bastien", "Colin", "teamtrainers.fr", "06 45 72 30 61", 14, today,
                plans["Carte 10 séances"], -95, -12, creditsRemaining: 0,
                notes: "Carte expirée, n'a pas renouvelé depuis le printemps."),
            ClientMember("Chloé", "Ferrand", "teamtrainers.fr", "06 11 58 42 90", 13, today,
                plans["Illimité mensuel"], -22, 8),
            ClientMember("Yanis", "Roussel", "teamtrainers.fr", "06 93 26 71 05", 12, today,
                plans["Illimité annuel"], -60, 305),
            ClientMember("Émilie", "Fabre", "teamtrainers.fr", "06 38 90 15 47", 11, today,
                plans["Illimité mensuel"], -14, 16),
            ClientMember("Nicolas", "Weber", "teamtrainers.fr", "06 70 43 28 12", 10, today,
                plans["Illimité mensuel"], -8, 22),
            ClientMember("Laura", "Guyot", "teamtrainers.fr", "06 24 61 83 55", 9, today,
                plans["Carte 10 séances"], -20, 40, creditsRemaining: 7),
            ClientMember("Tristan", "Meyer", "teamtrainers.fr", "06 57 12 39 84", 8, today,
                plans["Illimité mensuel"], -26, 4),
            ClientMember("Océane", "Vasseur", "teamtrainers.fr", "06 02 77 46 31", 7, today,
                plans["Illimité mensuel"], -12, 18),
            ClientMember("Adrien", "Pichon", "teamtrainers.fr", "06 66 34 09 27", 6, today,
                plans["Illimité annuel"], -30, 335),
            ClientMember("Manon", "Leclerc", "teamtrainers.fr", "06 48 21 65 73", 5, today,
                plans["Illimité mensuel"], -16, 14),
            ClientMember("Julien", "Barre", "teamtrainers.fr", "06 15 88 52 40", 4, today,
                plans["Illimité mensuel"], -3, 27),
            ClientMember("Sarah", "Nguyen", "teamtrainers.fr", "06 79 05 31 68", 3, today,
                plans["Carte 10 séances"], -45, 30, creditsRemaining: 4),
            ClientMember("Romain", "Teixeira", "teamtrainers.fr", "06 36 47 90 12", 45, today,
                plans["Illimité mensuel"], -20, 10, joinedDaysAgo: 21,
                notes: "Inscrit ce mois-ci, découvre les circuits du mardi.")
        };

        foreach (var member in members)
        {
            gym.AddMember(member);
        }

        // Six slots, none above fourteen seats: the room tops out at eighteen and
        // the pool at eighteen members, so a full evening class is full without
        // seating anybody twice.
        var schedule = new SeededWeek(
            [
                new("Team Strength", "Najate", DayOfWeek.Monday, 12, 30, 11),
                new("Cross Circuit", "Marine", DayOfWeek.Monday, 18, 30, 14),
                new("Cardio Boost", "Najate", DayOfWeek.Tuesday, 19, 0, 15),
                new("Cross Circuit", "Marine", DayOfWeek.Thursday, 7, 0, 8),
                new("Team Strength", "Marine", DayOfWeek.Friday, 18, 0, 16),
                new("Coaching duo", "Najate", DayOfWeek.Saturday, 10, 0, 2)
            ],
            new Dictionary<string, int>
            {
                ["Coaching duo"] = 97,
                ["Cross Circuit"] = 89,
                ["Team Strength"] = 83,
                ["Cardio Boost"] = 76
            },
            // Bastien (5), whose card ran out, and Tristan (11). Both offsets
            // clear 20 so they overlap RegularReliability's 120–220 band: below
            // that, a member is ranked last in every session they are seated in
            // and comes out at nought per cent with no last visit — somebody who
            // never came rather than somebody to chase.
            new Dictionary<int, int> { [5] = 60, [11] = 90 });

        dbContext.Sessions.AddRange(CreateSessions(templates, coaches, members, schedule));
    }

    /// <summary>
    /// Leyssa Coaching: one coach, tiny groups, half of it at the member's home.
    /// <para>
    /// The interesting shape here is the opposite of Team Trainer's — nothing is
    /// ever full because nothing seats more than six, and two of the three
    /// courses are one-to-one. It is also the only customer whose coach is the
    /// person who signs in, and whose Coachs section is hidden by
    /// <c>IsSolo</c> while its sessions still carry her name.
    /// </para>
    /// </summary>
    private static void FillLeyssaGym(GymDbContext dbContext, Gym gym)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // No Site at all: an itinerant coach has an area, not a building — the
        // same rule that gives the tenant an AreaLabel instead of a street.
        var studio = Studio(
            "Studio Rives", "Petit groupe", "sparkles", "brand", 6, 32m, "Rez-de-chaussée",
            "Salle louée à l'heure au bord du lac, pour les cours en petit comité.",
            ["Tapis ×6", "Briques", "Sangles", "Bolsters", "Parquet"]);

        var home = WithEquipment(
            new Location("À domicile")
            {
                Kind = LocationKind.Home,
                TypeLabel = "Chez le membre",
                IconKey = "home",
                Tone = "neutral",
                Capacity = 1,
                Note = "Séances individuelles au domicile du membre, dans la zone d'intervention. "
                       + "Najate apporte son matériel."
            },
            ["Matériel apporté par la coach"]);

        var lakeside = WithEquipment(
            new Location("Quai de Rives")
            {
                Kind = LocationKind.Outdoor,
                TypeLabel = "Plein air · mobilité",
                IconKey = "tree",
                Tone = "success",
                Capacity = 6,
                Note = "Séances de mobilité au bord du lac dès les beaux jours. "
                       + "Repli au studio en cas de pluie.",
                IsWeatherDependent = true,
                FallbackLocation = studio
            },
            ["Tapis transportables", "Élastiques"]);

        dbContext.Locations.AddRange(studio, home, lakeside);

        var disciplines = CreateClientDisciplines(
            ("Yoga", "sparkles", "success"),
            ("Pilates", "target", "brand"),
            ("Mobilité", "target", "success"),
            ("Coaching perso", "user", "neutral"));

        dbContext.Disciplines.AddRange(disciplines.Values);

        // The coach is the account holder. The Coachs section is hidden for a
        // solo customer, but her sessions still have to carry a name — which is
        // exactly the case a hidden navigation entry must not break.
        var najate = new Coach("Najate", "Amzil")
        {
            RoleLabel = "Coach · fondatrice",
            Email = "najate@leyssa-coaching.fr",
            Phone = "06 24 71 08 33",
            JoinedOn = today.AddMonths(-34),
            Bio = "Coach indépendante à Thonon. Najate travaille en petits groupes et "
                  + "à domicile, surtout sur la mobilité et le renforcement doux.",
            AvailableOnMonday = true, AvailableOnTuesday = true, AvailableOnWednesday = true,
            AvailableOnThursday = true, AvailableOnFriday = true, AvailableOnSaturday = true,
            AvailableOnSunday = false
        };
        najate.AddDiscipline(disciplines["Yoga"], 0);
        najate.AddDiscipline(disciplines["Pilates"], 1);
        najate.AddDiscipline(disciplines["Mobilité"], 2);
        najate.AddDiscipline(disciplines["Coaching perso"], 3);
        najate.AddCertification("Yoga Alliance 200h", 0);
        najate.AddCertification("Pilates Mat · niveau 2", 1);
        najate.AddCertification("PSC1 · premiers secours", 2);

        gym.AddCoach(najate);
        var coaches = new Dictionary<string, Coach> { ["Najate"] = najate };

        var plans = CreateClientPlans(
            new Plan
            {
                Name = "Suivi mensuel",
                ShortName = "Suivi",
                Price = 90m,
                Unit = "€ / mois",
                Kind = PlanKind.Recurring,
                ValidityMonths = 1,
                BillingLabel = "Sans engagement",
                Description = "Deux séances par semaine, en groupe ou à domicile.",
                Tone = "brand",
                IsFeatured = true,
                Rank = 0
            },
            new Plan
            {
                Name = "Carte 5 séances",
                ShortName = "Carte 5",
                Price = 250m,
                Unit = "€ / carte",
                Kind = PlanKind.CreditPack,
                CreditCount = 5,
                ValidityMonths = 3,
                BillingLabel = "Paiement unique",
                Description = "5 séances individuelles valables 3 mois.",
                Tone = "neutral",
                Rank = 1
            });

        dbContext.Plans.AddRange(plans.Values);

        var templates = new[]
        {
            ClientTemplate(
                "Yoga doux", disciplines["Yoga"], 60, 6, studio,
                CourseLevel.AllLevels, CourseIntensity.Gentle, price: null,
                "Séance lente en petit groupe, respiration et étirements.",
                [najate]),
            ClientTemplate(
                "Mobilité au lac", disciplines["Mobilité"], 45, 6, lakeside,
                CourseLevel.AllLevels, CourseIntensity.Gentle, price: null,
                "Mobilité articulaire en extérieur, face au lac. Repli au studio s'il pleut.",
                [najate]),
            ClientTemplate(
                "Coaching individuel", disciplines["Coaching perso"], 60, 1, home,
                CourseLevel.Custom, CourseIntensity.Private, price: 55m,
                "Séance individuelle au domicile du membre, adaptée à son objectif.",
                [najate])
        }.ToDictionary(template => template.Name);

        dbContext.CourseTemplates.AddRange(templates.Values);

        var members = new List<Member>
        {
            ClientMember("Élodie", "Bonnet", "example.fr", "06 82 14 60 37", 20, today,
                plans["Suivi mensuel"], -9, 21,
                notes: "Séances à domicile le mardi matin."),
            ClientMember("Rémi", "Charpentier", "example.fr", "06 47 03 91 25", 16, today,
                plans["Carte 5 séances"], -40, 50, creditsRemaining: 2),
            ClientMember("Inès", "Nadal", "example.fr", "06 30 58 27 14", 13, today,
                plans["Suivi mensuel"], -19, 11),
            ClientMember("Claire", "Mestre", "example.fr", "06 65 22 84 09", 9, today,
                plans["Suivi mensuel"], -4, 26),
            ClientMember("Fanny", "Dorel", "example.fr", "06 08 76 43 51", 6, today,
                plans["Carte 5 séances"], -80, -6, creditsRemaining: 0,
                notes: "Carte terminée, doit reprendre à la rentrée."),
            ClientMember("Antoine", "Ruiz", "example.fr", "06 91 37 15 62", 30, today,
                plans["Suivi mensuel"], -13, 17, joinedDaysAgo: 34)
        };

        foreach (var member in members)
        {
            gym.AddMember(member);
        }

        // Four slots for one coach, and none of them big. "Coaching individuel"
        // seats exactly one, which is what makes a private session read as
        // private rather than as an empty class.
        var schedule = new SeededWeek(
            [
                new("Yoga doux", "Najate", DayOfWeek.Monday, 9, 30, 5),
                new("Coaching individuel", "Najate", DayOfWeek.Tuesday, 10, 0, 1),
                new("Mobilité au lac", "Najate", DayOfWeek.Thursday, 18, 0, 6),
                new("Yoga doux", "Najate", DayOfWeek.Saturday, 10, 30, 6)
            ],
            new Dictionary<string, int>
            {
                ["Coaching individuel"] = 100,
                ["Yoga doux"] = 88,
                ["Mobilité au lac"] = 80
            },
            // Fanny (4), whose card ran out — same overlap rule as Team
            // Trainer's, and it bites harder here: with four slots and six
            // members she is seated in nearly everything.
            new Dictionary<int, int> { [4] = 65 });

        dbContext.Sessions.AddRange(CreateSessions(templates, coaches, members, schedule));
    }

    /// <summary>
    /// Builds a client's discipline referential from its own short list. Named
    /// apart from <see cref="CreateDisciplines"/> on purpose: a params overload
    /// of the same name would also match the no-argument call GymXYZ makes.
    /// </summary>
    private static Dictionary<string, Discipline> CreateClientDisciplines(
        params (string Name, string IconKey, string Tone)[] specs) =>
        specs
            .Select(spec => new Discipline(spec.Name) { IconKey = spec.IconKey, Tone = spec.Tone })
            .ToDictionary(discipline => discipline.Name);

    /// <summary>Keys a client's formules by name, the way <see cref="CreatePlans"/> does for GymXYZ.</summary>
    private static Dictionary<string, Plan> CreateClientPlans(params Plan[] plans) =>
        plans.ToDictionary(plan => plan.Name);

    /// <summary>
    /// A client's course, taking its venue and coaches as objects rather than by
    /// name — a small customer's catalogue is short enough to read inline, and
    /// the lookup dictionaries GymXYZ needs would be more ceremony than help.
    /// </summary>
    private static CourseTemplate ClientTemplate(
        string name,
        Discipline discipline,
        int durationMinutes,
        int capacity,
        Location defaultLocation,
        CourseLevel level,
        CourseIntensity intensity,
        decimal? price,
        string description,
        Coach[] coaches)
    {
        var template = new CourseTemplate(name)
        {
            Discipline = discipline,
            DurationMinutes = durationMinutes,
            Capacity = capacity,
            DefaultLocation = defaultLocation,
            Level = level,
            Intensity = intensity,
            Price = price,
            Description = description
        };

        for (var rank = 0; rank < coaches.Length; rank++)
        {
            template.AddCoach(coaches[rank], rank);
        }

        return template;
    }

    /// <summary>
    /// A client's member, with the same cover chain GymXYZ's people get — the
    /// e-mail domain is the only thing that changes, since a gym's members use
    /// their own addresses and a solo coach's are private individuals.
    /// </summary>
    private static Member ClientMember(
        string firstName,
        string lastName,
        string emailDomain,
        string phone,
        int joinedMonthsAgo,
        DateOnly today,
        Plan plan,
        int subscriptionStartsInDays,
        int subscriptionEndsInDays,
        int? creditsRemaining = null,
        string? notes = null,
        int? joinedDaysAgo = null) =>
        CreateMember(
            firstName, lastName,
            $"{Slug(firstName)}.{Slug(lastName)}@{emailDomain}", phone,
            joinedMonthsAgo, today, plan,
            subscriptionStartsInDays, subscriptionEndsInDays,
            creditsRemaining, autoRenew: true, notes, joinedDaysAgo);

    /// <summary>
    /// What the customer owes TechXYZ. One invoice a year at the plan's yearly
    /// rate, newest first on screen — the hand-off draws three of them.
    /// <para>
    /// Nothing collects this money and nothing generates a document: these rows
    /// exist so the billing panel has a history to list.
    /// </para>
    /// </summary>
    private static async Task SeedInvoicesAsync(GymDbContext dbContext, Tenant tenant)
    {
        if (tenant.PlanPrice is not { } monthly)
            return;

        var yearly = monthly * 12;
        var thisYear = DateTime.Today.Year;

        for (var age = 0; age < 3; age++)
        {
            var year = thisYear - age;

            dbContext.Invoices.Add(new Invoice
            {
                TenantId = tenant.Id,
                Reference = $"GX-{year}-{tenant.Id:000}",
                Date = new DateOnly(year, 1, 1),
                Amount = yearly,
                Status = InvoiceStatus.Paid
            });
        }

        await dbContext.SaveChangesAsync();
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

    /// <summary>
    /// The paramétrage of the Réglages screens: money, tax, opening hours and
    /// the six notification switches, set as the hand-off draws them.
    /// </summary>
    private static async Task SeedSettingsAsync(GymDbContext dbContext, Tenant tenant)
    {
        var settings = new GymSettings
        {
            Currency = GymSettings.DefaultCurrency,
            VatMention = "TVA non applicable, art. 293 B du CGI",
            AcceptedPaymentMethods =
            [
                PaymentMethod.Card,
                PaymentMethod.SepaDirectDebit,
                PaymentMethod.Cash,
                PaymentMethod.PaymentLink
            ],
            SchoolZone = SchoolZones.ForPostcode(tenant.ZipCode)
        };

        settings.AddOpeningHours(new OpeningHours
        {
            DayFrom = DayOfWeek.Monday,
            DayTo = DayOfWeek.Friday,
            OpensAt = new TimeOnly(6, 30),
            ClosesAt = new TimeOnly(22, 0)
        });

        settings.AddOpeningHours(new OpeningHours
        {
            DayFrom = DayOfWeek.Saturday,
            DayTo = DayOfWeek.Saturday,
            OpensAt = new TimeOnly(8, 0),
            ClosesAt = new TimeOnly(19, 0)
        });

        settings.AddOpeningHours(new OpeningHours
        {
            DayFrom = DayOfWeek.Sunday,
            DayTo = DayOfWeek.Sunday,
            OpensAt = new TimeOnly(9, 0),
            ClosesAt = new TimeOnly(13, 0)
        });

        dbContext.GymSettings.Add(settings);

        dbContext.NotificationSettings.AddRange(
            NotificationDefaults.All.Select(entry => NotificationDefaults.Create(entry.Key)));

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Who can sign in: part of the team holds an account, one collaborator is
    /// still waiting on their invitation, and the rest have never been asked.
    /// <para>
    /// Deliberately partial. A seed where everybody has an account would show
    /// the « Équipe &amp; accès » panel in the one state it never has to handle.
    /// </para>
    /// <para>
    /// No member signs in. Members are reached by e-mail rather than by an
    /// account, so seeding a member login would seed a role nothing is allowed
    /// to hold — and, until the screens were partitioned, that login opened the
    /// whole management console.
    /// </para>
    /// </summary>
    private static async Task SeedAccessAsync(
        IServiceProvider serviceProvider,
        GymDbContext dbContext,
        Tenant tenant)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var now = DateTime.UtcNow;

        async Task<string> CreateAccountAsync(
            string email,
            string displayName,
            string roleLabel,
            string role,
            int lastSeenHoursAgo)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = tenant.Id,
                DisplayName = displayName,
                RoleLabel = roleLabel,
                LastSeenAt = now.AddHours(-lastSeenHoursAgo)
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not seed the demo account {email}: {errors}");
            }

            await userManager.AddToRoleAsync(user, role);
            return user.Id;
        }

        // Three of the six coaches sign in; the other three have never been asked.
        var coaches = await dbContext.Coaches.ToListAsync();

        foreach (var (email, hoursAgo) in new[]
                 {
                     ("nora.lemoine@gymxyz.fr", 3),
                     ("samir.elamrani@gymxyz.fr", 2),
                     ("lea.fontaine@gymxyz.fr", 26)
                 })
        {
            if (coaches.FirstOrDefault(coach => coach.Email == email) is not { } coach)
                continue;

            coach.UserId = await CreateAccountAsync(
                email, $"{coach.FirstName} {coach.LastName}", coach.RoleLabel ?? "Coach",
                GymRoles.Coach, hoursAgo);
        }

        // No member account: Member.UserId stays null on all six, which is what
        // the « Comptes membres » panel now has to render for everybody.
        var members = await dbContext.Members.ToListAsync();
        var camille = members.FirstOrDefault(member => member.Email == "camille.durand@gymxyz.fr");

        dbContext.Invitations.AddRange(
            // A collaborator: Théo coaches here and has no account yet.
            new Invitation
            {
                Email = "theo.garnier@gymxyz.fr",
                RoleName = GymRoleNames.Coach,
                SentOn = now.AddDays(-2)
            },
            // Kept although a member cannot sign in: it is the one row that puts
            // the « Comptes membres » panel in its "invited" state, and the
            // e-mail invitation flow will read this table rather than a new one.
            new Invitation
            {
                Email = "camille.durand@gymxyz.fr",
                RoleName = GymRoleNames.Member,
                MemberId = camille?.Id,
                SentOn = now.AddDays(-5)
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedGymAsync(GymDbContext dbContext, Tenant tenant)
    {
        var gym = new Gym("GymXYZ Lyon 3ᵉ");
        tenant.AddGym(gym);

        var mainSite = new Site("GymXYZ Lyon 3ᵉ")
        {
            Address = new Address
            {
                Street = "14 rue de la Villette",
                ZipCode = "69003",
                City = "Lyon 3ᵉ",
                Country = "France"
            }
        };

        var locations = CreateLocations();

        // Only the indoor venues belong to the building. The park and the
        // member's home are venues of the gym without being rooms in it, which
        // is exactly why Location.SiteId is optional.
        foreach (var location in locations.Values.Where(location => location.Kind == LocationKind.Studio))
        {
            mainSite.AddLocation(location);
        }

        gym.AddSite(mainSite);

        dbContext.Locations.AddRange(
            locations.Values.Where(location => location.Kind != LocationKind.Studio));

        var disciplines = CreateDisciplines();
        dbContext.Disciplines.AddRange(disciplines.Values);

        var coaches = new Dictionary<string, Coach>();
        foreach (var coach in CreateDemoCoaches(disciplines))
        {
            gym.AddCoach(coach);
            coaches[coach.FirstName] = coach;
        }

        var plans = CreatePlans();
        dbContext.Plans.AddRange(plans.Values);

        // Ordered on purpose: the six people of the demo set come first, so they
        // are the ones filling the sessions the screens open on.
        var members = CreateDemoMembers(plans)
            .Concat(CreateSupportingMembers(plans))
            .ToList();

        foreach (var member in members)
        {
            gym.AddMember(member);
        }

        var demoMembers = members
            .Take(6)
            .ToDictionary(member => member.LastName);

        dbContext.Payments.AddRange(
            CreatePayments(demoMembers, DateOnly.FromDateTime(DateTime.Today)));

        var courseTemplates = CreateCourseTemplates(disciplines, locations, coaches)
            .ToDictionary(template => template.Name);
        dbContext.CourseTemplates.AddRange(courseTemplates.Values);

        dbContext.Sessions.AddRange(CreateSessions(courseTemplates, coaches, members, GymXyzWeek));

        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// The six venues of the design hand-off demo set, in the three natures the
    /// catalogue mixes: four studios, one outdoor spot and the member's home.
    /// <para>
    /// Occupancy, sessions per week, the day's schedule and the weekly heatmap
    /// are not stored on a venue: all four are counted from the sessions booked
    /// in it, and so is the "Forte demande" chip Studio C wears in the prototype.
    /// </para>
    /// </summary>
    private static Dictionary<string, Location> CreateLocations()
    {
        var studioA = Studio(
            "Studio A", "Cours collectifs", "grid", "brand", 20, 65m, "Rez-de-chaussée",
            "Grande salle polyvalente pour les formats collectifs — renforcement, HIIT, pilates.",
            ["Tapis ×20", "Steps", "Haltères", "Élastiques", "Miroir mural", "Sono"]);

        var studioB = Studio(
            "Studio B", "Yoga & mobilité", "sparkles", "success", 20, 48m, "1ᵉʳ étage",
            "Ambiance calme, parquet et lumière tamisée — dédiée au yoga, au pilates doux et à la mobilité.",
            ["Tapis ×20", "Briques", "Bolsters", "Sangles", "Parquet", "Lumière tamisée"]);

        var studioC = Studio(
            "Studio C", "Cycling & cardio", "target", "danger", 24, 55m, "Sous-sol",
            "Salle de cycling immersive — la plus demandée. Liste d'attente fréquente sur les créneaux du soir.",
            ["24 vélos", "Sono immersive", "Écran LED", "Ventilation", "Éclairage scénique"]);

        var openGym = Studio(
            "Espace libre", "Musculation & open gym", "dumbbell", "neutral", 30, 120m, "Rez-de-chaussée",
            "Plateau musculation en accès libre aux heures d'ouverture. Encadrement ponctuel sur les circuits.",
            ["Rack squat ×2", "Bancs", "Haltères 2–40 kg", "Poulies", "Tapis de course ×4", "Rameurs ×2"],
            isOpenAccess: true);

        var park = WithEquipment(
            new Location("Parc de la Tête d'Or")
            {
                Kind = LocationKind.Outdoor,
                TypeLabel = "Plein air · bootcamp",
                IconKey = "tree",
                Tone = "success",
                Capacity = 20,
                Note = "Cours en extérieur sur la grande pelouse dès les beaux jours — bootcamp, "
                       + "renforcement et cardio. Repli en salle en cas de pluie.",
                // The prototype writes the meeting point and nothing else, so
                // nothing else is invented here.
                Address = new Address
                {
                    Street = "Entrée Bd des Belges",
                    ZipCode = string.Empty,
                    City = string.Empty,
                    Country = string.Empty
                },
                IsWeatherDependent = true,
                // Through the navigation, not the key: neither venue has an id
                // before the insert.
                FallbackLocation = studioA
            },
            ["Matériel apporté", "Tapis transportables", "Kettlebells", "Élastiques", "Plots & cônes"]);

        var home = WithEquipment(
            new Location("À domicile")
            {
                Kind = LocationKind.Home,
                TypeLabel = "Chez le membre",
                IconKey = "home",
                Tone = "neutral",
                Capacity = 1,
                Note = "Séances individuelles au domicile du membre. L'adresse est renseignée sur la "
                       + "fiche du membre puis transmise au coach avant chaque rendez-vous — le coach "
                       + "apporte son matériel."
            },
            ["Matériel apporté par le coach"]);

        return new List<Location> { studioA, studioB, studioC, openGym, park, home }
            .ToDictionary(location => location.Name);
    }

    /// <summary>An indoor room, with its kit. Shared by every customer's venues.</summary>
    private static Location Studio(
        string name,
        string typeLabel,
        string iconKey,
        string tone,
        int capacity,
        decimal areaSqm,
        string floor,
        string note,
        string[] equipment,
        bool isOpenAccess = false) =>
        WithEquipment(
            new Location(name)
            {
                Kind = LocationKind.Studio,
                TypeLabel = typeLabel,
                IconKey = iconKey,
                Tone = tone,
                Capacity = capacity,
                AreaSqm = areaSqm,
                Floor = floor,
                Note = note,
                IsOpenAccess = isOpenAccess
            },
            equipment);

    private static Location WithEquipment(Location location, string[] equipment)
    {
        for (var rank = 0; rank < equipment.Length; rank++)
        {
            location.AddEquipment(equipment[rank], rank);
        }

        return location;
    }

    /// <summary>
    /// The disciplines the demo team teaches. No screen manages this
    /// referential yet, so it is seeded; the icon and tone keys are read by the
    /// course catalogue at lot 3.
    /// </summary>
    private static Dictionary<string, Discipline> CreateDisciplines()
    {
        Discipline Create(string name, string iconKey, string tone) =>
            new(name) { IconKey = iconKey, Tone = tone };

        return new List<Discipline>
        {
            Create("Pilates", "target", "brand"),
            Create("Yoga", "sparkles", "success"),
            Create("Mobilité", "target", "success"),
            Create("HIIT", "trend", "danger"),
            Create("Cardio", "trend", "warning"),
            Create("Renforcement", "dumbbell", "brand"),
            Create("Musculation", "dumbbell", "brand"),
            Create("Cross-training", "dumbbell", "warning"),
            Create("Core", "target", "brand"),
            Create("Coaching perso", "user", "neutral"),
            Create("Cycling", "target", "warning"),
            Create("Boxe", "shield", "danger")
        }.ToDictionary(discipline => discipline.Name);
    }

    /// <summary>
    /// The eight course templates of the design hand-off demo set. What the
    /// prototype writes as one label — "Cardio · HIIT", "Coaching individuel" —
    /// resolves here to the single closest discipline of the referential.
    /// <para>
    /// Sessions per week, average fill, regulars and the upcoming sessions are
    /// not stored on a template: they are counted from its occurrences, seeded
    /// by <see cref="CreateSessions"/>.
    /// </para>
    /// </summary>
    private static IEnumerable<CourseTemplate> CreateCourseTemplates(
        IReadOnlyDictionary<string, Discipline> disciplines,
        IReadOnlyDictionary<string, Location> locations,
        IReadOnlyDictionary<string, Coach> coaches)
    {
        CourseTemplate Create(
            string name,
            string disciplineName,
            int durationMinutes,
            int capacity,
            string studio,
            CourseLevel level,
            CourseIntensity intensity,
            decimal? price,
            string description,
            string[] coachFirstNames,
            string? iconKey = null)
        {
            var template = new CourseTemplate(name)
            {
                Discipline = disciplines[disciplineName],
                IconKey = iconKey,
                DurationMinutes = durationMinutes,
                Capacity = capacity,
                DefaultLocation = locations[studio],
                Level = level,
                Intensity = intensity,
                Price = price,
                Description = description
            };

            for (var rank = 0; rank < coachFirstNames.Length; rank++)
            {
                template.AddCoach(coaches[coachFirstNames[rank]], rank);
            }

            return template;
        }

        yield return Create(
            "HIIT Blast", "HIIT", 60, 16, "Studio A",
            CourseLevel.AllLevels, CourseIntensity.High, price: null,
            "Intervalles courts et intenses alternant cardio et renforcement. Format efficace en une heure, adaptable selon le niveau du groupe.",
            ["Nora", "Théo"]);

        yield return Create(
            "Power Cycle", "Cycling", 45, 24, "Studio C",
            CourseLevel.Intermediate, CourseIntensity.High, price: null,
            "Séance de vélo indoor rythmée par la musique. Le cours le plus demandé de la salle — liste d'attente fréquente.",
            ["Léa", "Nora"]);

        yield return Create(
            "Pilates Core", "Pilates", 50, 16, "Studio A",
            CourseLevel.AllLevels, CourseIntensity.Moderate, price: null,
            "Travail de gainage et de posture au sol. Renforce la sangle abdominale en profondeur, sans impact.",
            ["Nora", "Inès"]);

        yield return Create(
            "Yoga Restore", "Yoga", 60, 20, "Studio B",
            CourseLevel.AllLevels, CourseIntensity.Gentle, price: null,
            "Séance lente axée sur la respiration et les étirements. Idéale en fin de journée pour récupérer.",
            ["Inès", "Nora"]);

        yield return Create(
            "Strength Foundations", "Renforcement", 60, 20, "Studio A",
            CourseLevel.Beginner, CourseIntensity.Moderate, price: null,
            "Apprentissage des mouvements de base avec charges légères. Pose les fondations avant les formats plus exigeants.",
            ["Nora", "Karim"]);

        // The only course whose icon differs from its discipline's: the
        // prototype draws a target here, where Boxe carries a shield.
        yield return Create(
            "Boxing Fundamentals", "Boxe", 60, 16, "Studio C",
            CourseLevel.AllLevels, CourseIntensity.High, price: null,
            "Technique de boxe anglaise, déplacements et travail au sac. Cardio complet, sans contact.",
            ["Théo"],
            iconKey: "target");

        yield return Create(
            "Core Express", "Renforcement", 30, 18, "Studio B",
            CourseLevel.AllLevels, CourseIntensity.Moderate, price: null,
            "Format court et dense sur la pause déjeuner. Tout le travail de gainage en trente minutes.",
            ["Samir"]);

        yield return Create(
            "Coaching Perso", "Coaching perso", 60, 1, "Studio C",
            CourseLevel.Custom, CourseIntensity.Private, price: 45m,
            "Séance individuelle adaptée à l'objectif du membre. Réservable directement auprès du coach.",
            ["Samir", "Karim"]);
    }

    /// <summary>
    /// The six coaches of the design hand-off demo set, with the disciplines,
    /// certifications and weekly availability the prototype shows. Joining dates
    /// are relative to today so the demo never goes stale.
    /// <para>
    /// No standing is written here. Léa reads "Cours pleins" in the prototype
    /// and does so again here, but only because the sessions seeded for her fill
    /// up — the value is computed, never stored.
    /// </para>
    /// </summary>
    private static IEnumerable<Coach> CreateDemoCoaches(IReadOnlyDictionary<string, Discipline> disciplines)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        Coach Create(
            string firstName,
            string lastName,
            string roleLabel,
            string email,
            string phone,
            int joinedMonthsAgo,
            string bio,
            string[] disciplineNames,
            string[] certifications,
            bool[] availability,
            DateOnly? awayUntil = null)
        {
            var coach = new Coach(firstName, lastName)
            {
                RoleLabel = roleLabel,
                Email = email,
                Phone = phone,
                JoinedOn = today.AddMonths(-joinedMonthsAgo),
                Bio = bio,
                AwayUntil = awayUntil,
                AvailableOnMonday = availability[0],
                AvailableOnTuesday = availability[1],
                AvailableOnWednesday = availability[2],
                AvailableOnThursday = availability[3],
                AvailableOnFriday = availability[4],
                AvailableOnSaturday = availability[5],
                AvailableOnSunday = availability[6]
            };

            for (var rank = 0; rank < disciplineNames.Length; rank++)
            {
                coach.AddDiscipline(disciplines[disciplineNames[rank]], rank);
            }

            for (var rank = 0; rank < certifications.Length; rank++)
            {
                coach.AddCertification(certifications[rank], rank);
            }

            return coach;
        }

        yield return Create(
            "Nora", "Lemoine", "Coach senior · co-fondatrice",
            "nora.lemoine@gymxyz.fr", "06 41 22 18 07", joinedMonthsAgo: 53,
            "Pilier de la salle depuis l'ouverture. Nora alterne les cours doux du matin et les formats toniques du soir. Elle suit aussi les nouveaux membres sur leurs premières séances.",
            ["Pilates", "Yoga", "Mobilité", "HIIT"],
            ["BPJEPS AF — Cours collectifs", "Pilates Mat · niveau 2", "Yoga Alliance 200h"],
            [true, true, true, true, true, true, false]);

        yield return Create(
            "Samir", "El Amrani", "Coach renforcement",
            "samir.elamrani@gymxyz.fr", "06 55 70 33 12", joinedMonthsAgo: 45,
            "Spécialiste du renforcement et du suivi individuel. Samir gère la majorité des coachings privés et les formats express du midi.",
            ["Renforcement", "Coaching perso", "Core"],
            ["BPJEPS AGFF — Haltérophilie & musculation", "Préparation physique · FFHM"],
            [true, true, true, true, true, false, false]);

        yield return Create(
            "Théo", "Garnier", "Coach boxe & cardio",
            "theo.garnier@gymxyz.fr", "06 12 90 44 51", joinedMonthsAgo: 39,
            "Ancien compétiteur amateur, Théo anime les cours de boxe technique et les circuits cardio du soir. Très suivi par les habitués.",
            ["Boxe", "Cardio", "HIIT"],
            ["BPJEPS — Boxe anglaise", "PSC1 · premiers secours"],
            [false, false, false, false, false, true, true],
            awayUntil: today.AddDays(11));

        yield return Create(
            "Léa", "Fontaine", "Coach cycling",
            "lea.fontaine@gymxyz.fr", "06 88 21 67 02", joinedMonthsAgo: 32,
            "Ses séances de cycling affichent presque toujours complet. Léa entretient une liste d'attente fidèle et propose des sessions supplémentaires le week-end.",
            ["Cycling", "Cardio"],
            ["Indoor Cycling · Schwinn", "BPJEPS AF — Cours collectifs"],
            [true, true, true, false, true, true, true]);

        yield return Create(
            "Karim", "Bouaziz", "Coach musculation",
            "karim.bouaziz@gymxyz.fr", "06 73 50 29 88", joinedMonthsAgo: 28,
            "Encadre la salle de musculation et les circuits cross-training. Karim accompagne les débutants sur la technique des mouvements de base.",
            ["Musculation", "Cross-training"],
            ["BPJEPS AGFF", "Haltérophilie · initiateur"],
            [true, false, true, true, true, true, false]);

        yield return Create(
            "Inès", "Ravel", "Coach yoga & mobilité",
            "ines.ravel@gymxyz.fr", "06 30 14 76 95", joinedMonthsAgo: 25,
            "Arrivée récemment, Inès a relancé les créneaux yoga du matin et les ateliers mobilité du week-end, déjà très appréciés.",
            ["Yoga", "Mobilité", "Pilates"],
            ["Yoga Alliance 300h", "Mobilité fonctionnelle · FRC"],
            [true, true, false, true, true, true, true]);
    }

    /// <summary>
    /// The four formules of the prototype, in the order its cards are laid out —
    /// which is neither alphabetical nor by price, hence <see cref="Plan.Rank"/>.
    /// <para>
    /// "Carte 10 séances" is the one pack: ten entries, four months to use them.
    /// The other three run by the calendar and are what the MRR is the sum of —
    /// the pack is deliberately outside it, a one-off purchase being no more a
    /// monthly revenue than it is a monthly cost.
    /// </para>
    /// </summary>
    private static Dictionary<string, Plan> CreatePlans()
    {
        var plans = new[]
        {
            new Plan
            {
                Name = "Illimité mensuel",
                ShortName = "Illimité",
                Price = 49m,
                Unit = "€ / mois",
                Kind = PlanKind.Recurring,
                ValidityMonths = 1,
                BillingLabel = "Sans engagement",
                Description = "Accès illimité à tous les cours collectifs.",
                Tone = "brand",
                IsFeatured = true,
                Rank = 0
            },
            new Plan
            {
                Name = "Carte 10 séances",
                ShortName = "Carte 10",
                Price = 120m,
                Unit = "€ / carte",
                Kind = PlanKind.CreditPack,
                CreditCount = 10,
                ValidityMonths = 4,
                BillingLabel = "Paiement unique",
                Description = "10 entrées valables 4 mois.",
                Tone = "neutral",
                Rank = 1
            },
            new Plan
            {
                Name = "Étudiant mensuel",
                ShortName = "Étudiant",
                Price = 35m,
                Unit = "€ / mois",
                Kind = PlanKind.Recurring,
                ValidityMonths = 1,
                BillingLabel = "Sans engagement",
                Description = "Tarif réduit sur justificatif de scolarité.",
                Tone = "success",
                Rank = 2
            },
            new Plan
            {
                Name = "Illimité annuel",
                ShortName = "Annuel",
                Price = 490m,
                Unit = "€ / an",
                Kind = PlanKind.Recurring,
                ValidityMonths = 12,
                BillingLabel = "Engagement 12 mois",
                Description = "Deux mois offerts sur l'année.",
                Tone = "warning",
                Rank = 3
            }
        };

        return plans.ToDictionary(plan => plan.Name);
    }

    /// <summary>
    /// The six members of the design hand-off demo set, same people as on every
    /// other screen. Dates are relative to today so the demo never goes stale;
    /// the subscription windows are what produce the three standings shown in
    /// the prototype (four active, one expiring, one inactive).
    /// <para>
    /// The formules are the ones the abonnements screen gives each of them.
    /// Camille lands on "Expire bientôt" twice over — five days left and three
    /// entries left — which is the prototype's own row for her, and Théo's
    /// expired pack plus the rejected direct debit below is what makes him
    /// "En retard" there and "Inactif" on the members table.
    /// </para>
    /// </summary>
    private static IEnumerable<Member> CreateDemoMembers(Dictionary<string, Plan> plans)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        yield return CreateMember(
            "Laetitia", "Moriceau", "laetitia.moriceau@gymxyz.fr", "06 12 34 56 78",
            joinedMonthsAgo: 27, today, plans["Illimité mensuel"],
            subscriptionStartsInDays: -12, subscriptionEndsInDays: 18,
            notes: "Préfère les cours du matin. Vient surtout en début de semaine.");

        yield return CreateMember(
            "Camille", "Durand", "camille.durand@gymxyz.fr", "06 22 11 90 04",
            joinedMonthsAgo: 17, today, plans["Carte 10 séances"],
            subscriptionStartsInDays: -25, subscriptionEndsInDays: 5, creditsRemaining: 3);

        yield return CreateMember(
            "Lucas", "Martin", "lucas.martin@gymxyz.fr", "06 80 45 12 33",
            joinedMonthsAgo: 20, today, plans["Étudiant mensuel"],
            subscriptionStartsInDays: -40, subscriptionEndsInDays: 50);

        yield return CreateMember(
            "Amina", "Benali", "amina.benali@gymxyz.fr", "06 14 78 22 09",
            joinedMonthsAgo: 28, today, plans["Illimité annuel"],
            subscriptionStartsInDays: -8, subscriptionEndsInDays: 22);

        yield return CreateMember(
            "Théo", "Garnier", "theo.garnier@gymxyz.fr", "06 55 32 87 41",
            joinedMonthsAgo: 21, today, plans["Carte 10 séances"],
            subscriptionStartsInDays: -90, subscriptionEndsInDays: -25, creditsRemaining: 0);

        yield return CreateMember(
            "Sarah", "Cohen", "sarah.cohen@gymxyz.fr", "06 71 09 55 18",
            joinedMonthsAgo: 37, today, plans["Illimité mensuel"],
            subscriptionStartsInDays: -3, subscriptionEndsInDays: 27);
    }

    /// <summary>
    /// The five encaissements of the prototype's "Encaissements récents", over
    /// the last week and against the plans the demo six actually hold — the mock
    /// prints 49 € beside Amina, who it also puts on the yearly plan, and only
    /// one of those two can be true here.
    /// <para>
    /// Théo's rejected direct debit is the load-bearing one: without it his
    /// expired pack would merely read "Terminé", and "En retard" would have
    /// nothing behind it.
    /// </para>
    /// </summary>
    private static IEnumerable<Payment> CreatePayments(
        IReadOnlyDictionary<string, Member> members,
        DateOnly today)
    {
        Payment Record(string lastName, int daysAgo, PaymentMethod method, PaymentStatus status)
        {
            var member = members[lastName];
            var subscription = member.Subscriptions!.First();

            return new Payment
            {
                Member = member,
                Subscription = subscription,
                Date = today.AddDays(-daysAgo),
                Label = subscription.Plan!.Name,
                Amount = subscription.Plan.Price,
                Method = method,
                Status = status
            };
        }

        yield return Record("Benali", 1, PaymentMethod.SepaDirectDebit, PaymentStatus.Collected);
        yield return Record("Cohen", 2, PaymentMethod.Card, PaymentStatus.Collected);
        yield return Record("Durand", 3, PaymentMethod.Cash, PaymentStatus.Collected);
        yield return Record("Garnier", 4, PaymentMethod.SepaDirectDebit, PaymentStatus.Rejected);
        yield return Record("Martin", 5, PaymentMethod.Card, PaymentStatus.Collected);
    }

    /// <summary>
    /// The rest of the room. The prototype's occupancy figures — "14/20", a
    /// Power Cycle at "24/24" — need more people than the six the screens tell a
    /// story about, and every one of those figures is counted from real
    /// registrations. So the demo set gains a supporting cast: a name, an active
    /// subscription, nothing else. The prototype's own dashboard claims "112
    /// actifs", so this errs on the small side rather than the large one.
    /// </summary>
    private static IEnumerable<Member> CreateSupportingMembers(Dictionary<string, Plan> plans)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        string[][] people =
        [
            ["Chloé", "Perrin"], ["Maxime", "Roussel"], ["Inès", "Vasseur"], ["Julien", "Marchand"],
            ["Émilie", "Barbier"], ["Antoine", "Leroy"], ["Manon", "Dumas"], ["Hugo", "Chevalier"],
            ["Clara", "Renard"], ["Nathan", "Guerin"], ["Léna", "Masson"], ["Rayan", "Bertrand"],
            ["Jade", "Fournier", ], ["Enzo", "Lambert"], ["Alice", "Girard"], ["Malik", "Traoré"],
            ["Louise", "Dupuis"], ["Gabriel", "Meunier"], ["Zoé", "Blanchard"], ["Adam", "Colin"],
            ["Éva", "Marty"], ["Tom", "Robin"], ["Nina", "Charpentier"], ["Yanis", "Aubert"],
            ["Romane", "Poirier"], ["Ethan", "Baron"], ["Lola", "Rolland"], ["Sofiane", "Merle"],
            ["Anaïs", "Deschamps"], ["Victor", "Pichon"]
        ];

        for (var index = 0; index < people.Length; index++)
        {
            var firstName = people[index][0];
            var lastName = people[index][1];

            // Spread over the year so the joining dates on the list are not all
            // the same day, and spread over the four formules so the répartition
            // card has four bars of different lengths rather than two.
            var plan = index % 7 == 0 ? plans["Illimité annuel"]
                : index % 3 == 0 ? plans["Carte 10 séances"]
                : index % 5 == 0 ? plans["Étudiant mensuel"]
                : plans["Illimité mensuel"];

            // Two arrivals inside the last month, so the MRR has something
            // truthful to compare against. With the whole room signed up long
            // ago the figure would be flat month on month, and a gym that took
            // nobody on in thirty days is not the story this demo tells.
            //
            // One of them buys a pack, which moves the room but not the MRR —
            // the business rule made visible rather than asserted.
            var newcomer = index is 1 or 3;
            var startsInDays = newcomer ? -12 - index : -20 - index % 40;

            yield return CreateMember(
                firstName,
                lastName,
                $"{Slug(firstName)}.{Slug(lastName)}@gymxyz.fr",
                $"06 {30 + index:00} {11 + index:00} {40 + index:00} {index:00}",
                joinedMonthsAgo: 2 + index % 22,
                today,
                plan,
                subscriptionStartsInDays: startsInDays,
                subscriptionEndsInDays: 10 + index % 60,
                // Packs at every stage of being used up, so the gauges on the
                // members table are not all the same length.
                creditsRemaining: 10 - index % 9,
                // A member never holds a cover that predates their arrival, so
                // the newcomers join the day their first one starts.
                joinedDaysAgo: newcomer ? -startsInDays : null);
        }
    }

    /// <summary>Accents out of an e-mail address, without a culture surprise.</summary>
    private static string Slug(string name) =>
        string.Concat(name.Normalize(System.Text.NormalizationForm.FormD)
                .Where(character => char.GetUnicodeCategory(character)
                    != System.Globalization.UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant();

    private static Member CreateMember(
        string firstName,
        string lastName,
        string email,
        string phone,
        int joinedMonthsAgo,
        DateOnly today,
        Plan plan,
        int subscriptionStartsInDays,
        int subscriptionEndsInDays,
        int? creditsRemaining = null,
        bool autoRenew = true,
        string? notes = null,
        int? joinedDaysAgo = null)
    {
        // The current cover windows are the ones lot 1 seeded, kept to the day:
        // they are what produce the four active, one expiring and one inactive
        // the members table shows, and moving them would quietly restate a
        // screen three lots have already been checked against.
        // Months are the natural grain for "membre depuis mars 2024"; a recent
        // arrival needs days, because a month of granularity cannot say
        // "joined three weeks ago" without predating their own subscription.
        var joinedOn = joinedDaysAgo is { } days
            ? today.AddDays(-days)
            : today.AddMonths(-joinedMonthsAgo);
        var startedOn = today.AddDays(subscriptionStartsInDays);

        // Lot 1 sized every window at about a month, which was right when a
        // subscription was just two dates. A yearly formule cannot run for
        // thirty days: Amina Benali would read "Illimité annuel · 490 € / an"
        // above an échéance three weeks out, and the gauge would be filling
        // from the wrong denominator. The cover takes the period it was sold
        // with — and since every one of these is comfortably active either way,
        // no standing on the members table moves.
        //
        // Packs keep their seeded windows: those encode Théo Garnier's expired
        // card, which the "En retard" story on three screens depends on.
        var endsOn = plan.Kind == PlanKind.Recurring && plan.ValidityMonths > 1
            ? SubscriptionFactory.EndOfCover(plan, startedOn)
            : today.AddDays(subscriptionEndsInDays);

        var current = new Subscription
        {
            Plan = plan,
            StartedOn = startedOn,
            EndsOn = endsOn,
            CreditsRemaining = plan.IsCredited ? creditsRemaining ?? plan.CreditCount : null,
            CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
            AutoRenew = plan.Kind == PlanKind.Recurring && autoRenew,
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };

        return new Member(firstName, lastName)
        {
            Email = email,
            Phone = phone,
            JoinedOn = joinedOn,
            Notes = notes,
            Subscriptions = [current, .. PreviousCovers(current, plan, joinedOn)]
        };
    }

    /// <summary>
    /// The covers a member bought before the one running now, back to the day
    /// they joined.
    /// <para>
    /// Without these the database says every member bought exactly one
    /// subscription, ever — and the MRR comparison built on that is nonsense.
    /// "What was the recurring revenue a month ago" sums the covers running
    /// then, and on a monthly plan those are <b>different rows</b>: a cover that
    /// started more than thirty days ago has already ended. With one row per
    /// member the only covers old enough to count were the expired ones, so
    /// last month came out near empty and the delta read +73 %. Seeding the
    /// chain is not decoration — it is what makes the question answerable.
    /// </para>
    /// <para>
    /// Capped rather than exhaustive: a member of twenty-seven months would
    /// otherwise carry twenty-seven rows, and the record's history list is meant
    /// to be read. A year of it is plenty to answer every question the screens
    /// ask, and the join date still stops the chain when it comes first.
    /// </para>
    /// </summary>
    private static IEnumerable<Subscription> PreviousCovers(Subscription current, Plan plan, DateOnly joinedOn)
    {
        const int MaxHistory = 6;

        var months = Math.Max(1, plan.ValidityMonths);
        var cursor = current.StartedOn;

        for (var index = 0; index < MaxHistory && cursor > joinedOn; index++)
        {
            var endsOn = cursor.AddDays(-1);
            var startedOn = cursor.AddMonths(-months);

            // The first one a member ever bought starts the day they joined,
            // not before it.
            if (startedOn < joinedOn)
            {
                startedOn = joinedOn;
            }

            yield return new Subscription
            {
                Plan = plan,
                StartedOn = startedOn,
                EndsOn = endsOn,
                // A pack that has been superseded was used up: leaving entries
                // on it would have the members table counting credits somebody
                // can no longer spend.
                CreditsRemaining = plan.IsCredited ? 0 : null,
                CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
                AutoRenew = current.AutoRenew,
                PriceLabel = plan.FormatPriceLabel(),
                Price = plan.Price,
                MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
            };

            cursor = startedOn;
        }
    }

    /// <summary>
    /// One line of the demo week: a course, its coach, when it runs and how full
    /// it is. The venue is not written here — a session runs in the studio its
    /// course template proposes.
    /// </summary>
    private sealed record WeeklySlot(
        string CourseName,
        string CoachFirstName,
        DayOfWeek Day,
        int Hour,
        int Minute,
        int Occupancy);

    /// <summary>
    /// Everything about a customer's week that differs from another's: the slots
    /// themselves, how well each course is attended, and who the chronic
    /// absentees are.
    /// <para>
    /// These three were static fields describing GymXYZ until lot 11 gave the two
    /// client brands a real timetable of their own. They travel together because
    /// they are read together — the rates are keyed by the course names the slots
    /// mention, and the absentee indices point into the same member list the
    /// slots are filled from.
    /// </para>
    /// </summary>
    private sealed record SeededWeek(
        WeeklySlot[] Slots,
        IReadOnlyDictionary<string, int> AttendanceRates,
        IReadOnlyDictionary<int, int> ChronicAbsentees);

    /// <summary>
    /// GymXYZ's week, unchanged from lot 1. Its figures are what three lots of
    /// screenshots were checked against, so nothing here may move when a client
    /// gets a timetable.
    /// </summary>
    private static SeededWeek GymXyzWeek =>
        new(DemoWeek, CourseAttendanceRates, ChronicAbsentees);

    /// <summary>
    /// The demo week, taken from the coaches' weekly schedules in the prototype.
    /// <para>
    /// Only courses the catalogue actually holds are here. The prototype's
    /// planning also names Mobility Reset, Strength Lab, Cross Circuit and Open
    /// Gym, which its own course catalogue does not list — the mock is
    /// hand-tuned, not generated, and a session is an occurrence of a catalogue
    /// course. Inventing four templates would have added four rows to the Cours
    /// screen that the prototype does not show.
    /// </para>
    /// <para>
    /// The occupancy figures are what make the story on the other screens true:
    /// Studio C comes out the busiest venue and wears "Forte demande", Léa is
    /// the one coach whose sessions read "Cours pleins", and Power Cycle fills
    /// up with a waiting list behind it. Samir sits deliberately clear of the
    /// threshold rather than on it — a demo where a chip depends on rounding is
    /// a demo that changes its mind.
    /// </para>
    /// </summary>
    private static readonly WeeklySlot[] DemoWeek =
    [
        new("Strength Foundations", "Nora", DayOfWeek.Monday, 7, 15, 16),
        new("Core Express", "Samir", DayOfWeek.Monday, 12, 30, 15),
        new("Strength Foundations", "Karim", DayOfWeek.Monday, 17, 0, 15),
        new("Power Cycle", "Léa", DayOfWeek.Monday, 18, 30, 24),
        new("Yoga Restore", "Inès", DayOfWeek.Tuesday, 8, 0, 16),
        new("Pilates Core", "Nora", DayOfWeek.Tuesday, 18, 0, 13),
        new("Coaching Perso", "Samir", DayOfWeek.Wednesday, 11, 0, 1),
        new("Power Cycle", "Léa", DayOfWeek.Wednesday, 12, 30, 22),
        new("Boxing Fundamentals", "Théo", DayOfWeek.Wednesday, 19, 30, 11),
        new("Yoga Restore", "Nora", DayOfWeek.Thursday, 17, 15, 14),
        new("Coaching Perso", "Karim", DayOfWeek.Thursday, 18, 0, 1),
        new("HIIT Blast", "Nora", DayOfWeek.Friday, 9, 0, 13),
        new("Power Cycle", "Nora", DayOfWeek.Friday, 18, 0, 24),
        new("Power Cycle", "Léa", DayOfWeek.Saturday, 10, 0, 23),
        new("HIIT Blast", "Nora", DayOfWeek.Sunday, 9, 0, 9),
        new("Yoga Restore", "Inès", DayOfWeek.Sunday, 10, 30, 18)
    ];

    /// <summary>Weeks of history the demo carries, so past figures have something to average.</summary>
    private const int SeededWeeksBehind = 4;

    /// <summary>Weeks ahead, so the planning can be navigated forward and members have courses to come.</summary>
    private const int SeededWeeksAhead = 8;

    /// <summary>Two names kept warm behind a full session, as the prototype describes Léa's cycling.</summary>
    private const int WaitlistDepth = 2;

    /// <summary>
    /// How well each course is attended, as a percentage of the seats that were
    /// taken. These are what the "taux par cours" bars read, and the order is the
    /// prototype's: Power Cycle in front, Boxing Fundamentals last.
    /// <para>
    /// A course is not equally well attended just because it is equally full —
    /// that is the whole point of the screen, so the figures deliberately do not
    /// track the occupancies in <see cref="DemoWeek"/>.
    /// </para>
    /// </summary>
    private static readonly Dictionary<string, int> CourseAttendanceRates = new()
    {
        ["Power Cycle"] = 96,
        ["Coaching Perso"] = 94,
        ["Core Express"] = 91,
        ["HIIT Blast"] = 86,
        ["Strength Foundations"] = 84,
        ["Yoga Restore"] = 82,
        ["Pilates Core"] = 78,
        ["Boxing Fundamentals"] = 71
    };

    /// <summary>
    /// The three the "absents à relancer" card is about, keyed by their index in
    /// the seeded member list, with how far below everybody else they sit.
    /// Théo Garnier (4) misses most of it, Camille Durand (1) roughly a third,
    /// Maxime Roussel (7) rather less.
    /// <para>
    /// The offsets deliberately overlap <see cref="RegularReliability"/>. A flat
    /// "always the first to be marked absent" made Camille miss every single
    /// session of the demo — nought per cent and no last visit at all, which is
    /// not somebody to chase but somebody who never came. The prototype's own
    /// card reads "5 absences / 6", "3 / 8", "3 / 9": people who turn up
    /// sometimes.
    /// </para>
    /// <para>
    /// Two of the three are the prototype's own. Its third, "Léa Dubois", is not
    /// a member the catalogue holds; Maxime Roussel stands in, and is on the
    /// prototype's rosters too.
    /// </para>
    /// </summary>
    private static readonly Dictionary<int, int> ChronicAbsentees = new()
    {
        [4] = 0,
        [1] = 55,
        [7] = 85
    };

    /// <summary>Where everybody else sits — high enough to be picked last, low enough to overlap.</summary>
    private const int RegularReliability = 120;

    /// <summary>How long after the end of a session the sheet gets validated.</summary>
    private const int SheetClosedAfterMinutes = 15;

    /// <summary>
    /// Rolls the demo week out over the seeding horizon, one row per occurrence.
    /// Every occurrence of the same slot shares a <c>SeriesId</c>, which is what
    /// makes "this one and all the following" a single query later on.
    /// <para>
    /// The current week carries the prototype's figures exactly; the other weeks
    /// wobble by a couple of seats so the averages are not a flat line. Members
    /// are drawn from the pool at a fixed offset per slot, so the same faces come
    /// back week after week — which is what makes "habitués" mean anything.
    /// </para>
    /// </summary>
    private static IEnumerable<Session> CreateSessions(
        IReadOnlyDictionary<string, CourseTemplate> courseTemplates,
        IReadOnlyDictionary<string, Coach> coaches,
        IReadOnlyList<Member> members,
        SeededWeek schedule)
    {
        var today = DateTime.Today;
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        for (var slotIndex = 0; slotIndex < schedule.Slots.Length; slotIndex++)
        {
            var slot = schedule.Slots[slotIndex];
            var template = courseTemplates[slot.CourseName];
            var seriesId = Guid.NewGuid();

            for (var week = -SeededWeeksBehind; week <= SeededWeeksAhead; week++)
            {
                var dayOffset = ((int)slot.Day + 6) % 7;
                var startsAt = monday
                    .AddDays(week * 7 + dayOffset)
                    .AddHours(slot.Hour)
                    .AddMinutes(slot.Minute);

                var occupancy = week == 0
                    ? slot.Occupancy
                    : Math.Clamp(slot.Occupancy + Wobble(week, slotIndex), 1, template.Capacity);

                var session = new Session
                {
                    CourseTemplate = template,
                    Coach = coaches[slot.CoachFirstName],
                    Location = template.DefaultLocation!,
                    StartsAt = startsAt,
                    EndsAt = startsAt.AddMinutes(template.DurationMinutes),
                    // Copied, not read through the template: editing the
                    // catalogue must not rewrite what already happened.
                    Capacity = template.Capacity,
                    Status = SessionStatus.Scheduled,
                    SeriesId = seriesId,
                    Registrations = []
                };

                var seats = occupancy;
                if (occupancy >= template.Capacity && template.Capacity > 1)
                {
                    seats += WaitlistDepth;
                }

                // Seats are drawn from the member pool by rotation, so asking for
                // more seats than the customer has members would sit the same
                // person twice in one room. GymXYZ never comes close — 36 members
                // against a 24-seat studio — but a small customer easily does.
                seats = Math.Min(seats, members.Count);

                var offset = (slotIndex * 7 + week + members.Count) % members.Count;
                for (var seat = 0; seat < seats; seat++)
                {
                    session.Registrations.Add(new Registration
                    {
                        Member = members[(offset + seat) % members.Count],
                        RegisteredAt = startsAt.AddDays(-3),
                        IsWaitlisted = seat >= occupancy
                    });
                }

                // Anything before today has been through the sheet; today's stays
                // open so the screen has something to point when it is first
                // opened.
                if (startsAt.Date < today)
                {
                    MarkAttendance(session, slot.CourseName, offset, members.Count, slotIndex, week, schedule);
                }

                yield return session;
            }
        }
    }

    /// <summary>Between -2 and +2 seats, decided by the week and the slot rather than by chance.</summary>
    private static int Wobble(int week, int slotIndex) => ((week * 3 + slotIndex) % 5 + 5) % 5 - 2;

    /// <summary>
    /// Runs a past session through its attendance sheet and validates it.
    /// <para>
    /// The seats are ranked by <see cref="Reliability"/> and the least reliable
    /// are the ones marked absent, as many as the course's rate calls for. Doing
    /// it by rank rather than by a per-seat draw is what makes the "taux par
    /// cours" bars land on the intended figures instead of near them, and what
    /// concentrates the absences on <see cref="ChronicAbsentees"/> rather than
    /// sprinkling them over the whole room.
    /// </para>
    /// <para>
    /// Waiting-list seats are left <see cref="AttendanceStatus.Pending"/>: the
    /// member never got in, so there was nothing to point.
    /// </para>
    /// </summary>
    private static void MarkAttendance(
        Session session,
        string courseName,
        int offset,
        int memberCount,
        int slotIndex,
        int week,
        SeededWeek schedule)
    {
        var seated = session.Registrations!
            .Select((registration, seat) => (registration, memberIndex: (offset + seat) % memberCount))
            .Where(seat => !seat.registration.IsWaitlisted)
            .OrderBy(seat => Reliability(seat.memberIndex, slotIndex, week, schedule.ChronicAbsentees))
            .ToList();

        if (seated.Count == 0)
        {
            return;
        }

        var rate = schedule.AttendanceRates.GetValueOrDefault(courseName, 85);
        var attended = (int)Math.Round(seated.Count * rate / 100d, MidpointRounding.AwayFromZero);
        var absent = seated.Count - attended;

        // One or two of those who did come turned up after the start. Small
        // classes get none: a nine-seat room where two people are late reads as
        // a broken course rather than a busy morning.
        var late = attended >= 8 ? 2 : attended >= 4 ? 1 : 0;

        for (var rank = 0; rank < seated.Count; rank++)
        {
            var registration = seated[rank].registration;

            if (rank < absent)
            {
                registration.Status = AttendanceStatus.Absent;
                continue;
            }

            // The least reliable of those who showed up are the ones who showed
            // up late, so the same faces recur there too.
            var isLate = rank < absent + late;
            registration.Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present;
            registration.CheckedInAt = isLate
                ? session.StartsAt.AddMinutes(4 + rank % 9)
                : session.StartsAt.AddMinutes(-(3 + rank % 8));
        }

        session.AttendanceClosedAt = session.EndsAt.AddMinutes(SheetClosedAfterMinutes);
    }

    /// <summary>
    /// How likely a member is to turn up, as a sortable rank rather than a
    /// probability. The jitter spans the gap between the bands, so a member who
    /// usually misses can still turn up on a good week and a regular can still
    /// skip one — which is what keeps the demo from reading as two castes.
    /// </summary>
    private static int Reliability(
        int memberIndex,
        int slotIndex,
        int week,
        IReadOnlyDictionary<int, int> chronicAbsentees)
    {
        var jitter = ((memberIndex * 13 + slotIndex * 29 + week * 7) % 101 + 101) % 101;

        return jitter + chronicAbsentees.GetValueOrDefault(memberIndex, RegularReliability);
    }
}
