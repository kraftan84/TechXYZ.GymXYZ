using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The figures lots 2 to 4 left as "—" and the planning fills: venue occupancy
/// and weekly slots, course fill and regulars, a coach's week. Every one of them
/// is counted from sessions and registrations, never stored, and these pin what
/// they are counted over.
/// </summary>
public class SessionFiguresQueryTests
{
    /// <summary>
    /// The venue card reads its occupancy from the seats taken in it, and its
    /// slot count from the week in progress alone.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldCountOccupancyAndWeeklySlots()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetLocations_ShouldCountOccupancyAndWeeklySlots));
        var studio = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.Add(studio);

        // The slot count reads the week on screen, the rate reads what has run,
        // and the week's own sessions cross from one to the other as the days
        // pass — so they all carry the same fill and the rate holds whatever day
        // this test runs on.
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Monday, 9), capacity: 20, registered: 12);
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Tuesday, 9), capacity: 20, registered: 12);

        Seed(dbContext, studio, LastWeek(DayOfWeek.Monday, 9), capacity: 20, registered: 12);
        Seed(dbContext, studio, LastWeek(DayOfWeek.Tuesday, 9), capacity: 20, registered: 12);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);
        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        var card = result.Items.Single();
        card.SessionsPerWeek.ShouldBe(2);
        card.OccupancyRate.ShouldBe(60);
        result.AverageStudioOccupancy.ShouldBe(60);
        result.TotalSessionsPerWeek.ShouldBe(2);
        card.Status.ShouldBe(LocationStatus.Available);
    }

    /// <summary>
    /// A studio whose sessions come back nearly full wears "Forte demande" — the
    /// prototype's chip, and the value lot 4 could not produce.
    /// </summary>
    [Fact]
    public async Task GetLocations_ShouldReadHighDemand_WhenTheStudioFillsUp()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetLocations_ShouldReadHighDemand_WhenTheStudioFillsUp));
        var busy = new Location("Studio C") { Kind = LocationKind.Studio, Capacity = 24 };
        var quiet = new Location("Studio B") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.AddRange(busy, quiet);

        Seed(dbContext, busy, LastWeek(DayOfWeek.Monday, 18), capacity: 24, registered: 24);
        Seed(dbContext, quiet, LastWeek(DayOfWeek.Monday, 10), capacity: 20, registered: 8);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationsQueryHandler(dbContext);
        var result = await handler.Handle(new GetLocationsQuery(), CancellationToken.None);

        result.Items.Single(item => item.Name == "Studio C").Status.ShouldBe(LocationStatus.HighDemand);
        result.Items.Single(item => item.Name == "Studio B").Status.ShouldBe(LocationStatus.Available);
    }

    /// <summary>
    /// The venue record lights up its two empty cards: the day's schedule, and
    /// the heatmap — seven values, Monday to Sunday, a quiet day included.
    /// </summary>
    [Fact]
    public async Task GetLocationDetails_ShouldFillTheDayScheduleAndTheHeatmap()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetLocationDetails_ShouldFillTheDayScheduleAndTheHeatmap));
        var studio = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        dbContext.Locations.Add(studio);

        var coach = new Coach("Nora", "Lemoine");
        var todayAt = DateTime.Today.AddHours(18);

        Seed(dbContext, studio, todayAt, capacity: 20, registered: 10, courseName: "Yoga Restore", coach: coach);
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Monday, 9), capacity: 20, registered: 5);
        await dbContext.SaveChangesAsync();

        var handler = new GetLocationDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetLocationDetailsPageQuery(studio.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Today.ShouldContain(session =>
            session.CourseName == "Yoga Restore" &&
            session.Time == "18:00" &&
            session.CoachName == "Nora Lemoine" &&
            session.Registered == 10);

        result.Occupancy.HasHeatmap.ShouldBeTrue();
        result.Occupancy.DailyRates.Count.ShouldBe(7);
        result.Occupancy.DailyRates[0].ShouldBe(25);
        result.Occupancy.SessionsPerWeek.ShouldBe(2);
    }

    /// <summary>
    /// The catalogue row shows the average fill of what the course has already
    /// run. A session still filling up is deliberately left out: counting the
    /// two sign-ups a class three weeks away has would say the course is empty
    /// when it is only early.
    /// </summary>
    [Fact]
    public async Task GetCourseTemplates_ShouldAverageWhatHasAlreadyRun()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCourseTemplates_ShouldAverageWhatHasAlreadyRun));
        var studio = new Location("Studio A") { Capacity = 20 };
        var template = NewTemplate("HIIT Blast", 16);
        dbContext.CourseTemplates.Add(template);

        Seed(dbContext, studio, LastWeek(DayOfWeek.Monday, 9), capacity: 16, registered: 12, template: template);
        Seed(dbContext, studio, LastWeek(DayOfWeek.Friday, 9), capacity: 16, registered: 4, template: template);

        // Barely booked, and it has not happened: it must not drag the average.
        Seed(dbContext, studio, NextWeek(DayOfWeek.Monday, 9), capacity: 16, registered: 1, template: template);
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCourseTemplatesQuery(), CancellationToken.None);

        result.Items.Single().FillRate.ShouldBe(50);
    }

    /// <summary>
    /// The course record: how often it runs, how full it comes back, how many
    /// people come back to it, and what is next on the planning.
    /// </summary>
    [Fact]
    public async Task GetCourseTemplateDetails_ShouldFillTheStatsAndTheNextSessions()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCourseTemplateDetails_ShouldFillTheStatsAndTheNextSessions));
        var studio = new Location("Studio C") { Capacity = 24 };
        var template = NewTemplate("Power Cycle", 24);
        dbContext.CourseTemplates.Add(template);

        var regular = new Member("Laetitia", "Moriceau");
        var newcomer = new Member("Camille", "Durand");

        var past = Seed(dbContext, studio, LastWeek(DayOfWeek.Monday, 18), capacity: 24, registered: 0, template: template);
        past.Registrations = [Seat(regular), Seat(newcomer)];

        var alsoPast = Seed(dbContext, studio, LastWeek(DayOfWeek.Friday, 18), capacity: 24, registered: 0, template: template);
        alsoPast.Registrations = [Seat(regular)];

        Seed(dbContext, studio, NextWeek(DayOfWeek.Monday, 18), capacity: 24, registered: 6, template: template);
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplateDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetCourseTemplateDetailsPageQuery(template.Id), CancellationToken.None);

        result.ShouldNotBeNull();

        // Two seats over two sessions of 24: a low rate, but a real one.
        result!.Stats.FillRate.ShouldBe(6);

        // Only Laetitia came back, so only Laetitia is a regular.
        result.Stats.Regulars.ShouldBe(1);

        result.NextSessions.Count.ShouldBe(1);
        result.NextSessions[0].Time.ShouldBe("18:00");
        result.NextSessions[0].LocationName.ShouldBe("Studio C");
    }

    /// <summary>
    /// The coach grid: sessions run this week, average fill, and the standing
    /// that follows from it.
    /// </summary>
    [Fact]
    public async Task GetCoaches_ShouldCountTheWeekAndReadFullClasses()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldCountTheWeekAndReadFullClasses));
        var studio = new Location("Studio C") { Capacity = 24 };
        var lea = new Coach("Léa", "Fontaine");
        var theo = new Coach("Théo", "Garnier");
        dbContext.Coaches.AddRange(lea, theo);

        // The rate reads what has already run; the count reads the week on
        // screen. Two different windows on purpose, so each is seeded its own.
        Seed(dbContext, studio, LastWeek(DayOfWeek.Monday, 18), capacity: 24, registered: 24, coach: lea);
        Seed(dbContext, studio, LastWeek(DayOfWeek.Wednesday, 18), capacity: 24, registered: 23, coach: lea);
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Monday, 18), capacity: 24, registered: 20, coach: lea);
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Wednesday, 18), capacity: 24, registered: 20, coach: lea);

        Seed(dbContext, studio, LastWeek(DayOfWeek.Thursday, 19), capacity: 16, registered: 8, coach: theo);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        var fontaine = result.Items.Single(item => item.LastName == "Fontaine");
        fontaine.ClassesPerWeek.ShouldBe(2);
        fontaine.FillRate!.Value.ShouldBeGreaterThanOrEqualTo(90);
        fontaine.Status.ShouldBe(CoachStatus.FullClasses);

        var garnier = result.Items.Single(item => item.LastName == "Garnier");
        garnier.FillRate.ShouldBe(50);
        garnier.Status.ShouldBe(CoachStatus.Available);
    }

    /// <summary>The prototype's last chip: the fullest classes first.</summary>
    [Fact]
    public async Task GetCoaches_ShouldSortByFillRate_WhenAsked()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldSortByFillRate_WhenAsked));
        var studio = new Location("Studio C") { Capacity = 24 };
        var abbott = new Coach("Anna", "Abbott");
        var zulu = new Coach("Zoé", "Zulu");
        dbContext.Coaches.AddRange(abbott, zulu);

        Seed(dbContext, studio, ThisWeek(DayOfWeek.Monday, 18), capacity: 20, registered: 4, coach: abbott);
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Tuesday, 18), capacity: 20, registered: 18, coach: zulu);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);

        var alphabetical = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);
        alphabetical.Items.Select(item => item.LastName).ShouldBe(["Abbott", "Zulu"]);

        var byFill = await handler.Handle(
            new GetCoachesQuery { SortByFillRate = true }, CancellationToken.None);
        byFill.Items.Select(item => item.LastName).ShouldBe(["Zulu", "Abbott"]);
    }

    /// <summary>The coach record: the week's classes, and the head count behind them.</summary>
    [Fact]
    public async Task GetCoachDetails_ShouldFillTheWeekAndTheStats()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachDetails_ShouldFillTheWeekAndTheStats));
        var studio = new Location("Studio C") { Capacity = 24 };
        var coach = new Coach("Léa", "Fontaine");
        dbContext.Coaches.Add(coach);

        var laetitia = new Member("Laetitia", "Moriceau");
        var camille = new Member("Camille", "Durand");

        // The week list is the week on screen, and nobody is signed up to it yet
        // — so the head count below can only come from the sessions already run.
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Monday, 18),
            capacity: 24, registered: 0, coach: coach, courseName: "Power Cycle");
        Seed(dbContext, studio, ThisWeek(DayOfWeek.Wednesday, 12),
            capacity: 24, registered: 0, coach: coach, courseName: "Power Cycle");

        // …while the head count is of people already seen. The same person twice
        // is one member followed, not two.
        var past = Seed(dbContext, studio, LastWeek(DayOfWeek.Monday, 18),
            capacity: 24, registered: 0, coach: coach, courseName: "Power Cycle");
        past.Registrations = [Seat(laetitia), Seat(camille)];

        var alsoPast = Seed(dbContext, studio, LastWeek(DayOfWeek.Wednesday, 12),
            capacity: 24, registered: 0, coach: coach, courseName: "Power Cycle");
        alsoPast.Registrations = [Seat(laetitia)];

        await dbContext.SaveChangesAsync();

        var handler = new GetCoachDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachDetailsPageQuery(coach.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.WeekSessions.Count.ShouldBe(2);
        result.WeekSessions[0].DayLabel.ShouldBe("Lun");
        result.WeekSessions[0].Time.ShouldBe("18:00");
        result.WeekSessions[0].CourseName.ShouldBe("Power Cycle");

        result.Stats.ClassesPerWeek.ShouldBe(2);
        result.Stats.FollowedMembers.ShouldBe(2);
    }

    private static DateTime ThisWeek(DayOfWeek day, int hour) =>
        PlanningRules.MondayOf(DateTime.Today).AddDays(((int)day + 6) % 7).AddHours(hour);

    private static DateTime LastWeek(DayOfWeek day, int hour) => ThisWeek(day, hour).AddDays(-7);

    private static DateTime NextWeek(DayOfWeek day, int hour) => ThisWeek(day, hour).AddDays(7);

    private static Registration Seat(Member member) => new() { Member = member };

    private static CourseTemplate NewTemplate(string name, int capacity) =>
        new(name)
        {
            Discipline = new Discipline($"{name} discipline"),
            Capacity = capacity,
            DurationMinutes = 60
        };

    private static Session Seed(
        GymDbContext dbContext,
        Location location,
        DateTime startsAt,
        int capacity,
        int registered,
        CourseTemplate? template = null,
        Coach? coach = null,
        string courseName = "Cours")
    {
        var session = new Session
        {
            CourseTemplate = template ?? NewTemplate(courseName, capacity),
            Location = location,
            Coach = coach,
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            Capacity = capacity,
            Registrations = [.. Enumerable
                .Range(0, registered)
                .Select(seat => new Registration { Member = new Member($"Member{seat}", "Test") })]
        };

        dbContext.Sessions.Add(session);

        return session;
    }
}
