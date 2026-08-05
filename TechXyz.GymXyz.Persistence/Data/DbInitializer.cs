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

        var rooms = new List<Room> { new("Studio A"), new("Studio B"), new("Studio C") }
            .ToDictionary(room => room.Name);

        foreach (var room in rooms.Values)
        {
            mainSite.AddRoom(room);
        }

        gym.AddSite(mainSite);

        var disciplines = CreateDisciplines();
        dbContext.Disciplines.AddRange(disciplines.Values);

        var coaches = new Dictionary<string, Coach>();
        foreach (var coach in CreateDemoCoaches(disciplines))
        {
            gym.AddCoach(coach);
            coaches[coach.FirstName] = coach;
        }

        foreach (var member in CreateDemoMembers())
        {
            gym.AddMember(member);
        }

        dbContext.CourseTemplates.AddRange(CreateCourseTemplates(disciplines, rooms, coaches));

        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();
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
    /// deliberately absent: they are counted from past occurrences, which the
    /// planning produces at lot 5.
    /// </para>
    /// </summary>
    private static IEnumerable<CourseTemplate> CreateCourseTemplates(
        IReadOnlyDictionary<string, Discipline> disciplines,
        IReadOnlyDictionary<string, Room> rooms,
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
                DefaultRoom = rooms[studio],
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
    /// Léa reads "Disponible" here where the prototype says "Cours pleins":
    /// that standing is computed from the fill rate of her sessions, which the
    /// planning produces at lot 5.
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
