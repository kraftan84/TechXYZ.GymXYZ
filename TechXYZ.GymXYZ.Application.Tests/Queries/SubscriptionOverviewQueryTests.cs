using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The Abonnements screen in one read — and the MRR rule the business settled
/// before any of it was written: the monthly-normalised sum of the recurring
/// covers running today, packs excluded.
/// </summary>
public class SubscriptionOverviewQueryTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task Mrr_ShouldNormaliseByPeriodAndLeavePacksOut()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Mrr_ShouldNormaliseByPeriodAndLeavePacksOut));

        var monthly = TestPlans.Monthly();
        var yearly = Yearly();
        var pack = TestPlans.Pack();
        dbContext.Plans.AddRange(monthly, yearly, pack);

        Sell(dbContext, monthly, "Mensuel", startsInDays: -10, endsInDays: 20);
        Sell(dbContext, yearly, "Annuel", startsInDays: -30, endsInDays: 300);
        Sell(dbContext, pack, "Carte", startsInDays: -10, endsInDays: 100, credits: 8);
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        // 49 for the month + 490/12 for the year. The pack contributes nothing:
        // it is bought once, and smoothing it in would have the figure claim
        // money that will not come again.
        overview.Kpis.Mrr.ShouldBe(49m + decimal.Round(490m / 12, 2));
    }

    [Fact]
    public async Task Mrr_ShouldNotMoveWhenTheFormulePriceDoes()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Mrr_ShouldNotMoveWhenTheFormulePriceDoes));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);
        Sell(dbContext, monthly, "Ancien", startsInDays: -10, endsInDays: 20);
        await dbContext.SaveChangesAsync();

        (await Handle(dbContext)).Kpis.Mrr.ShouldBe(49m);

        // The gym raises its price. What the member already on the plan
        // contributes must not be restated — that is the whole reason the
        // monthly figure is snapshot on the subscription.
        monthly.Price = 59m;
        await dbContext.SaveChangesAsync();

        (await Handle(dbContext)).Kpis.Mrr.ShouldBe(49m);
    }

    [Fact]
    public async Task Kpis_ShouldCountWhatIsLateAndWhatItIsWorth()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Kpis_ShouldCountWhatIsLateAndWhatItIsWorth));

        var monthly = TestPlans.Monthly();
        var pack = TestPlans.Pack();
        dbContext.Plans.AddRange(monthly, pack);

        Sell(dbContext, monthly, "Actif", startsInDays: -10, endsInDays: 20);
        Sell(dbContext, monthly, "Expire", startsInDays: -25, endsInDays: 3);
        var lapsed = Sell(dbContext, pack, "Retard", startsInDays: -90, endsInDays: -4, credits: 0);
        lapsed.Payments =
        [
            new Payment
            {
                Member = lapsed.Member,
                Date = Today.AddDays(-6),
                Label = pack.Name,
                Amount = 120m,
                Method = PaymentMethod.SepaDirectDebit,
                Status = PaymentStatus.Rejected
            }
        ];
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        overview.Kpis.ActiveCount.ShouldBe(2);
        overview.Kpis.ExpiringCount.ShouldBe(1);
        overview.Kpis.LateCount.ShouldBe(1);

        // "180 € à encaisser" is the sum of what the late covers cost, not their
        // monthly-normalised share.
        overview.Kpis.LateAmount.ShouldBe(120m);
        overview.Kpis.MemberCount.ShouldBe(3);
    }

    [Fact]
    public async Task Subscriptions_ShouldIncludeLapsedCovers()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Subscriptions_ShouldIncludeLapsedCovers));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);
        Sell(dbContext, monthly, "Echu", startsInDays: -90, endsInDays: -20);
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        // "En retard" is a row of this table: leaving expired covers out would
        // empty the very filter the screen exists to work through.
        overview.Subscriptions.Count.ShouldBe(1);
        overview.Subscriptions[0].Status.ShouldBe(SubscriptionStatus.Ended);
    }

    [Fact]
    public async Task Subscriptions_ShouldShowOneRowPerMember_NotOnePerRenewal()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Subscriptions_ShouldShowOneRowPerMember_NotOnePerRenewal));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);

        // A member who has renewed twice: three covers, one person. The suivi
        // asks where the member stands, not what they have ever bought.
        var member = new Member("Laetitia", "Moriceau");
        dbContext.Members.Add(member);
        foreach (var (starts, ends) in new[] { (-70, -41), (-40, -11), (-10, 20) })
        {
            dbContext.Subscriptions.Add(new Subscription
            {
                Member = member,
                Plan = monthly,
                StartedOn = Today.AddDays(starts),
                EndsOn = Today.AddDays(ends),
                PriceLabel = monthly.FormatPriceLabel(),
                Price = monthly.Price,
                MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(monthly)
            });
        }
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        var row = overview.Subscriptions.ShouldHaveSingleItem();

        // And the one shown is the cover in force, not the oldest still inside
        // the window — the same "healthiest one" the members table picks.
        row.Status.ShouldBe(SubscriptionStatus.Active);
        row.Cover.EndsOn.ShouldBe(Today.AddDays(20));
    }

    [Fact]
    public async Task Mrr_ShouldCompareAgainstTheCoversRunningAMonthAgo()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Mrr_ShouldCompareAgainstTheCoversRunningAMonthAgo));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);

        // One long-standing member, renewed so they were covered last month by
        // the previous row — the whole reason the seed carries a history.
        var loyal = new Member("Sarah", "Cohen");
        dbContext.Members.Add(loyal);
        foreach (var (starts, ends) in new[] { (-40, -11), (-10, 20) })
        {
            dbContext.Subscriptions.Add(new Subscription
            {
                Member = loyal, Plan = monthly,
                StartedOn = Today.AddDays(starts), EndsOn = Today.AddDays(ends),
                PriceLabel = monthly.FormatPriceLabel(),
                Price = monthly.Price, MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(monthly)
            });
        }

        // And one who signed up a fortnight ago, so was paying nothing then.
        Sell(dbContext, monthly, "Nouvelle", startsInDays: -14, endsInDays: 16);
        await dbContext.SaveChangesAsync();

        var kpis = (await Handle(dbContext)).Kpis;

        // 98 today against 49 a month ago: the growth is the newcomer, and the
        // member who merely renewed contributes nothing to the delta. Without
        // the previous cover they would have counted as new too, and the figure
        // would read +100 % every month for a gym that grew by one.
        kpis.Mrr.ShouldBe(98m);
        kpis.MrrDeltaPercent.ShouldBe(100);
    }

    [Fact]
    public async Task Subscriptions_ShouldLeaveOutACoverBookedForLater()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Subscriptions_ShouldLeaveOutACoverBookedForLater));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);
        Sell(dbContext, monthly, "Reserve", startsInDays: 20, endsInDays: 50);
        await dbContext.SaveChangesAsync();

        (await Handle(dbContext)).Subscriptions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Payments_ShouldOnlyCoverTheLastWeek()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Payments_ShouldOnlyCoverTheLastWeek));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);
        var cover = Sell(dbContext, monthly, "Payeur", startsInDays: -10, endsInDays: 20);

        cover.Payments =
        [
            Record(cover, daysAgo: 1),
            Record(cover, daysAgo: 6),
            // The card's own caption says "7 derniers jours"; this one is older.
            Record(cover, daysAgo: 30)
        ];
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        overview.Payments.Count.ShouldBe(2);
        overview.Payments[0].Date.ShouldBe(Today.AddDays(-1));
        overview.Payments[0].FullName.ShouldBe("Membre Payeur");
    }

    [Fact]
    public async Task Plans_ShouldCarryTheirCoveredMembers()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Plans_ShouldCarryTheirCoveredMembers));

        var monthly = TestPlans.Monthly();
        dbContext.Plans.Add(monthly);
        Sell(dbContext, monthly, "Un", startsInDays: -10, endsInDays: 20);
        Sell(dbContext, monthly, "Deux", startsInDays: -10, endsInDays: 20);
        await dbContext.SaveChangesAsync();

        var overview = await Handle(dbContext);

        overview.Plans.Single().MemberCount.ShouldBe(2);

        // "Panier moyen par abonné actif" — the MRR spread over the people still
        // covered, which is what the card says it is.
        overview.Kpis.AverageBasket.ShouldBe(49m);
    }

    private static async Task<SubscriptionOverviewDto> Handle(GymDbContext dbContext) =>
        await new GetSubscriptionOverviewQueryHandler(dbContext)
            .Handle(new GetSubscriptionOverviewQuery(), CancellationToken.None);

    private static Plan Yearly() => new()
    {
        Name = "Illimité annuel",
        ShortName = "Annuel",
        Price = 490m,
        Unit = "€ / an",
        Kind = PlanKind.Recurring,
        ValidityMonths = 12,
        BillingLabel = "Engagement 12 mois",
        Rank = 3
    };

    private static Subscription Sell(
        GymDbContext dbContext,
        Plan plan,
        string lastName,
        int startsInDays,
        int endsInDays,
        int? credits = null)
    {
        var subscription = new Subscription
        {
            Member = new Member("Membre", lastName),
            Plan = plan,
            StartedOn = Today.AddDays(startsInDays),
            EndsOn = Today.AddDays(endsInDays),
            CreditsRemaining = plan.IsCredited ? credits ?? plan.CreditCount : null,
            CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };

        dbContext.Subscriptions.Add(subscription);

        return subscription;
    }

    private static Payment Record(Subscription cover, int daysAgo) => new()
    {
        Member = cover.Member,
        Date = Today.AddDays(-daysAgo),
        Label = "Illimité mensuel",
        Amount = 49m,
        Method = PaymentMethod.Card,
        Status = PaymentStatus.Collected
    };
}
