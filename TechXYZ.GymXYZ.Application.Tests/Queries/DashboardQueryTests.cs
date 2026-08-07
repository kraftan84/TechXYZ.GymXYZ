using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The Accueil in one read.
/// <para>
/// The tests that matter most here are the last three: they ask the same seeded
/// gym the same question through the dashboard and through the screen each alert
/// leads to, and require the same answer. That is what stops the Accueil growing
/// a second definition of "expiring", "late" or "to point" — the one risk a
/// screen made of other screens' figures actually runs.
/// </para>
/// </summary>
public class DashboardQueryTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task Week_ShouldKeepSevenDays_EvenWhereNothingIsOn()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Week_ShouldKeepSevenDays_EvenWhereNothingIsOn));

        var monday = PlanningRules.MondayOf(Today);
        SeedSession(dbContext, monday.ToDateTime(new TimeOnly(9, 0)));
        SeedSession(dbContext, monday.ToDateTime(new TimeOnly(18, 0)));
        SeedSession(dbContext, monday.AddDays(2).ToDateTime(new TimeOnly(19, 0)));
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);

        // Seven cells, always. The strip is a week, not a list of busy days.
        dashboard.Week.Count.ShouldBe(7);
        dashboard.Week[0].SessionCount.ShouldBe(2);
        dashboard.Week[1].SessionCount.ShouldBe(0);
        dashboard.Week[2].SessionCount.ShouldBe(1);
        dashboard.WeekSessionCount.ShouldBe(3);
        dashboard.Week.Count(day => day.IsToday).ShouldBe(1);
    }

    /// <summary>
    /// A called-off class is not a class that runs. Counting it would have the
    /// strip promise a day's work that nobody is going to do.
    /// </summary>
    [Fact]
    public async Task Week_ShouldLeaveOutCancelledSessions()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Week_ShouldLeaveOutCancelledSessions));

        var slot = Today.ToDateTime(new TimeOnly(9, 0));
        SeedSession(dbContext, slot);

        var cancelled = SeedSession(dbContext, slot.AddHours(2));
        cancelled.Status = SessionStatus.Cancelled;
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);

        dashboard.WeekSessionCount.ShouldBe(1);
        dashboard.TodayClasses.Count.ShouldBe(1);
    }

    [Fact]
    public async Task TodayClasses_ShouldCarryOccupancyAndTellPrivateFromFull()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(TodayClasses_ShouldCarryOccupancyAndTellPrivateFromFull));

        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(9, 0)), booked: 12, capacity: 16, courseName: "HIIT Blast");
        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(11, 0)), booked: 1, capacity: 1, courseName: "Coaching Perso");
        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(18, 30)), booked: 24, capacity: 24, courseName: "Power Cycle");
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);

        dashboard.TodayClasses.Count.ShouldBe(3);

        // In the order they run: the card is read down the day.
        dashboard.TodayClasses.Select(session => session.CourseName)
            .ShouldBe(["HIIT Blast", "Coaching Perso", "Power Cycle"]);

        var hiit = dashboard.TodayClasses[0];
        hiit.FillPercent.ShouldBe(75);
        hiit.IsFull.ShouldBeFalse();
        hiit.IsPrivate.ShouldBeFalse();

        // A one-to-one is full the moment it is booked, and saying "Complet"
        // about it would be reporting a problem that is not one.
        dashboard.TodayClasses[1].IsPrivate.ShouldBeTrue();
        dashboard.TodayClasses[1].IsFull.ShouldBeFalse();

        dashboard.TodayClasses[2].IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task TodayClasses_ShouldNotReachIntoTheRestOfTheWeek()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(TodayClasses_ShouldNotReachIntoTheRestOfTheWeek));

        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(9, 0)), courseName: "Aujourd'hui");

        // Somewhere else in the same week, whichever day today happens to be.
        var elsewhere = PlanningRules.MondayOf(Today) == Today
            ? Today.AddDays(1)
            : PlanningRules.MondayOf(Today);
        SeedSession(dbContext, elsewhere.ToDateTime(new TimeOnly(9, 0)), courseName: "Ailleurs");
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);

        dashboard.TodayClasses.Select(session => session.CourseName).ShouldBe(["Aujourd'hui"]);
        dashboard.WeekSessionCount.ShouldBe(2);
    }

    /// <summary>
    /// Coaches on the week's schedule, not the size of the team: a coach away
    /// this week is not running anything, and « · 6 coachs » says who is on.
    /// </summary>
    [Fact]
    public async Task WeekCoachCount_ShouldCountWhoIsActuallyOn()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(WeekCoachCount_ShouldCountWhoIsActuallyOn));

        var nora = new Coach("Nora", "Lemoine");
        var samir = new Coach("Samir", "El Amrani");

        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(9, 0)), coach: nora);
        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(12, 0)), coach: nora);
        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(18, 0)), coach: samir);

        // An open-access slot nobody animates must not count as a coach.
        SeedSession(dbContext, Today.ToDateTime(new TimeOnly(20, 0)));
        await dbContext.SaveChangesAsync();

        (await Handle(dbContext)).WeekCoachCount.ShouldBe(2);
    }

    // ---- The three that pin the Accueil to the screens it points at ---------

    /// <summary>
    /// A member who has renewed several times holds several covers. « N
    /// abonnements expirent » is a statement about people, so it has to come out
    /// the same as the Abonnements suivi — counting the entity instead would
    /// give a larger number for the same gym on the same day.
    /// </summary>
    [Fact]
    public async Task ExpiringCount_ShouldMatchTheAbonnementsScreen_EvenWithRenewals()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(ExpiringCount_ShouldMatchTheAbonnementsScreen_EvenWithRenewals));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);

        // One member, two covers, the running one about to lapse.
        var member = new Member("Camille", "Durand");
        Sell(dbContext, monthly, member, startsInDays: -60, endsInDays: -30);
        Sell(dbContext, monthly, member, startsInDays: -30, endsInDays: 3);

        // A second member, plainly expiring.
        Sell(dbContext, monthly, new Member("Yanis", "Aubert"), startsInDays: -30, endsInDays: 2);
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);
        var overview = await Overview(dbContext);

        dashboard.Alerts.ExpiringCount.ShouldBe(overview.Kpis.ExpiringCount);
        dashboard.Alerts.ExpiringCount.ShouldBe(2);
    }

    [Fact]
    public async Task LateCountAndAmount_ShouldMatchTheAbonnementsScreen()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(LateCountAndAmount_ShouldMatchTheAbonnementsScreen));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);

        // Over, and owing: a payment that came back and nothing collected since.
        var owing = Sell(dbContext, monthly, new Member("Rémi", "Charpentier"), startsInDays: -60, endsInDays: -2);
        owing.Payments =
        [
            new Payment
            {
                Member = owing.Member,
                Date = Today.AddDays(-30),
                Label = "Illimité mensuel",
                Amount = 49m,
                Method = PaymentMethod.Card,
                Status = PaymentStatus.Rejected
            }
        ];

        // Over and settled — ended, not late.
        var settled = Sell(dbContext, monthly, new Member("Inès", "Nadal"), startsInDays: -60, endsInDays: -2);
        settled.Payments =
        [
            new Payment
            {
                Member = settled.Member,
                Date = Today.AddDays(-30),
                Label = "Illimité mensuel",
                Amount = 49m,
                Method = PaymentMethod.Card,
                Status = PaymentStatus.Collected
            }
        ];
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);
        var overview = await Overview(dbContext);

        dashboard.Alerts.LateCount.ShouldBe(overview.Kpis.LateCount);
        dashboard.Alerts.LateAmount.ShouldBe(overview.Kpis.LateAmount);
        dashboard.Alerts.LateCount.ShouldBe(1);
        dashboard.Alerts.LateAmount.ShouldBe(49m);
    }

    /// <summary>
    /// The figure the alert raises, the figure the Présences KPI shows and the
    /// figure the navigation badge draws are one number, taken from one place.
    /// </summary>
    [Fact]
    public async Task SheetsToPoint_ShouldMatchThePresencesScreen()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(SheetsToPoint_ShouldMatchThePresencesScreen));

        // Two open sheets inside the window, one of them forgotten days ago.
        SeedSession(dbContext, DateTime.Today.AddHours(-4), booked: 8);
        SeedSession(dbContext, DateTime.Today.AddDays(-3).AddHours(18), booked: 6);

        // Closed, so nothing to point.
        var closed = SeedSession(dbContext, DateTime.Today.AddDays(-1).AddHours(9), booked: 5);
        closed.AttendanceClosedAt = DateTime.Now.AddDays(-1);

        // Older than the window: past chasing, and it stops nagging.
        SeedSession(dbContext, DateTime.Today.AddDays(-AttendanceRules.ForgottenSheetDays - 1).AddHours(9));
        await dbContext.SaveChangesAsync();

        var dashboard = await Handle(dbContext);
        var presences = await AttendanceOverview(dbContext);

        dashboard.Alerts.SheetsToPoint.ShouldBe(presences.Kpis.SheetsToPoint);
        dashboard.Alerts.SheetsToPoint.ShouldBe(2);
    }

    /// <summary>The Abonnements badge: covers expiring plus covers late.</summary>
    [Fact]
    public void SubscriptionsToWatch_ShouldAddTheTwoCoverAlerts()
    {
        var alerts = new TechXyz.GymXyz.Application.Models.DashboardAlertsDto(4, 2, 180m, 3);

        // The prototype's sidebar draws 6 against exactly this 4 and 2.
        alerts.SubscriptionsToWatch.ShouldBe(6);
    }

    // ---- Helpers -----------------------------------------------------------

    private static Task<TechXyz.GymXyz.Application.Models.DashboardDto> Handle(GymDbContext dbContext) =>
        new GetDashboardQueryHandler(dbContext, TestCurrentUserService.Manager()).Handle(new GetDashboardQuery(), CancellationToken.None);

    private static Task<TechXyz.GymXyz.Application.Models.SubscriptionOverviewDto> Overview(GymDbContext dbContext) =>
        new GetSubscriptionOverviewQueryHandler(dbContext)
            .Handle(new GetSubscriptionOverviewQuery(), CancellationToken.None);

    private static Task<TechXyz.GymXyz.Application.Models.AttendanceOverviewDto> AttendanceOverview(GymDbContext dbContext) =>
        new GetAttendanceOverviewQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetAttendanceOverviewQuery(), CancellationToken.None);

    private static Session SeedSession(
        GymDbContext dbContext,
        DateTime startsAt,
        int booked = 0,
        int capacity = 24,
        string courseName = "Cours",
        Coach? coach = null)
    {
        var seats = new List<Registration>();
        for (var seat = 0; seat < booked; seat++)
        {
            seats.Add(new Registration { Member = new Member($"Membre{seat}", "Test") });
        }

        var session = new Session
        {
            CourseTemplate = new CourseTemplate(courseName)
            {
                Discipline = new Discipline($"{courseName} discipline"),
                DurationMinutes = 60,
                Capacity = capacity
            },
            Location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = capacity },
            Coach = coach,
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            Capacity = capacity,
            Registrations = seats
        };

        dbContext.Sessions.Add(session);

        return session;
    }

    private static Subscription Sell(
        GymDbContext dbContext,
        Plan plan,
        Member member,
        int startsInDays,
        int endsInDays)
    {
        var subscription = new Subscription
        {
            Member = member,
            Plan = plan,
            StartedOn = Today.AddDays(startsInDays),
            EndsOn = Today.AddDays(endsInDays),
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };

        dbContext.Subscriptions.Add(subscription);

        return subscription;
    }
}
