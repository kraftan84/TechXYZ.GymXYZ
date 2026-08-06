using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The formules on sale. One list, read by the plan picker on a member's record
/// today, by the abonnements cards next, and by the settings panel at lot 8 —
/// which is why nothing here filters.
/// </summary>
public class PlanQueriesHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task GetPlans_ShouldComeOutInDisplayOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetPlans_ShouldComeOutInDisplayOrder));

        // Added out of order on purpose: the grid's order is a commercial
        // choice, neither alphabetical nor by price.
        dbContext.Plans.AddRange(
            TestPlans.Pack(),
            TestPlans.Monthly());
        await dbContext.SaveChangesAsync();

        var plans = await new GetPlansQueryHandler(dbContext).Handle(new GetPlansQuery(), CancellationToken.None);

        plans.Select(plan => plan.Name).ShouldBe(["Illimité mensuel", "Carte 10 séances"]);
        plans[0].IsFeatured.ShouldBeTrue();
        plans[0].PriceLabel.ShouldBe("49 € / mois");
    }

    [Fact]
    public async Task GetPlans_ShouldCountTheMembersCoveredRightNow()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetPlans_ShouldCountTheMembersCoveredRightNow));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);

        // Covered, lapsed, and booked for next month. Only the first is a member
        // "on" the plan — the card says "64 membres", not "64 ever bought it".
        dbContext.Members.AddRange(
            WithCover(monthly, "Actif", startsInDays: -10, endsInDays: 20),
            WithCover(monthly, "Echu", startsInDays: -90, endsInDays: -30),
            WithCover(monthly, "Reserve", startsInDays: 10, endsInDays: 40));
        await dbContext.SaveChangesAsync();

        var plans = await new GetPlansQueryHandler(dbContext).Handle(new GetPlansQuery(), CancellationToken.None);

        plans.Single().MemberCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetPlans_ShouldLeaveOutARetiredFormule()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetPlans_ShouldLeaveOutARetiredFormule));

        var retired = TestPlans.Pack();
        retired.IsActive = false;
        dbContext.Plans.AddRange(TestPlans.Monthly(), retired);
        await dbContext.SaveChangesAsync();

        var plans = await new GetPlansQueryHandler(dbContext).Handle(new GetPlansQuery(), CancellationToken.None);

        plans.Count.ShouldBe(1);
        plans[0].Name.ShouldBe("Illimité mensuel");
    }

    [Fact]
    public void MonthlyRevenue_ShouldNormaliseRecurringPlansAndIgnorePacks()
    {
        // The MRR rule of lot 7: recurring only, normalised to the month. A pack
        // is a single purchase and contributes nothing — smoothing it in would
        // have the figure claim money that will not come again.
        var yearly = Plan(price: 490m, PlanKind.Recurring, validityMonths: 12);
        var monthly = Plan(price: 49m, PlanKind.Recurring, validityMonths: 1);
        var pack = Plan(price: 120m, PlanKind.CreditPack, validityMonths: 4);

        yearly.MonthlyRevenue.ShouldBe(490m / 12);
        monthly.MonthlyRevenue.ShouldBe(49m);
        pack.MonthlyRevenue.ShouldBe(0m);
    }

    private static TechXyz.GymXyz.Application.Models.PlanDto Plan(
        decimal price,
        PlanKind kind,
        int validityMonths) =>
        new(1, "Formule", "F", price, "€ / mois", kind, null, validityMonths, "Sans engagement",
            null, null, false, 0, 0);

    private static Member WithCover(Plan plan, string lastName, int startsInDays, int endsInDays) =>
        new("Membre", lastName)
        {
            Subscriptions =
            [
                new Subscription
                {
                    Plan = plan,
                    StartedOn = Today.AddDays(startsInDays),
                    EndsOn = Today.AddDays(endsInDays),
                    PriceLabel = plan.FormatPriceLabel()
                }
            ]
        };
}
