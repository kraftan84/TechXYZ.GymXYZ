using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The standing of a subscription is written twice, like the member's:
/// <c>Resolve</c> produces the chip, <c>Matches</c> filters in SQL. These tests
/// pin them to each other on the relational provider, so a rule that cannot be
/// translated fails here rather than in production.
/// </summary>
public class SubscriptionStatusRulesTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Horizon = SubscriptionStatusRules.HorizonFrom(Today);

    [Theory]
    // A recurring cover, by the calendar alone.
    [InlineData(PlanKind.Recurring, 120, null, false, SubscriptionStatus.Active)]
    [InlineData(PlanKind.Recurring, 8, null, false, SubscriptionStatus.Active)]
    [InlineData(PlanKind.Recurring, 7, null, false, SubscriptionStatus.ExpiringSoon)]
    [InlineData(PlanKind.Recurring, 0, null, false, SubscriptionStatus.ExpiringSoon)]
    [InlineData(PlanKind.Recurring, -1, null, false, SubscriptionStatus.Ended)]
    // The same dates, with money outstanding: over becomes late.
    [InlineData(PlanKind.Recurring, -1, null, true, SubscriptionStatus.Late)]
    [InlineData(PlanKind.Recurring, 120, null, true, SubscriptionStatus.Active)]
    // A pack runs out two ways, and either one is enough.
    [InlineData(PlanKind.CreditPack, 120, 10, false, SubscriptionStatus.Active)]
    [InlineData(PlanKind.CreditPack, 120, 4, false, SubscriptionStatus.Active)]
    [InlineData(PlanKind.CreditPack, 120, 3, false, SubscriptionStatus.ExpiringSoon)]
    [InlineData(PlanKind.CreditPack, 120, 1, false, SubscriptionStatus.ExpiringSoon)]
    [InlineData(PlanKind.CreditPack, 120, 0, false, SubscriptionStatus.Ended)]
    [InlineData(PlanKind.CreditPack, 120, 0, true, SubscriptionStatus.Late)]
    [InlineData(PlanKind.CreditPack, -4, 6, true, SubscriptionStatus.Late)]
    public void Resolve_ShouldReadTheCalendarAndTheEntriesAndTheMoney(
        PlanKind kind,
        int endsInDays,
        int? creditsRemaining,
        bool hasOutstandingPayment,
        SubscriptionStatus expected)
    {
        var cover = Cover(kind, endsInDays, creditsRemaining, hasOutstandingPayment);

        SubscriptionStatusRules.Resolve(cover, Today, Horizon).ShouldBe(expected);
    }

    [Fact]
    public void ThresholdsShouldBeTheOnesThePrototypeDraws()
    {
        SubscriptionStatusRules.ExpiringSoonWithinDays.ShouldBe(7);

        // Camille Durand, "3 séances restantes", warned.
        SubscriptionStatusRules.ExpiringSoonAtOrBelowCredits.ShouldBe(3);
    }

    [Fact]
    public async Task Matches_ShouldAgreeWithResolve_OnEveryBoundary_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var monthly = TestPlans.Monthly();
        var pack = TestPlans.Pack();
        dbContext.Plans.AddRange(monthly, pack);

        // Every leg of the rule, and the combinations where two of them
        // disagree — a pack with entries left whose calendar has run out, a
        // cover in good standing carrying a rejected payment.
        (int EndsInDays, int? Credits, bool Rejected)[] cases =
        [
            (120, null, false), (8, null, false), (7, null, false), (0, null, false),
            (-1, null, false), (-1, null, true), (120, null, true),
            (120, 10, false), (120, 3, false), (120, 0, false), (120, 0, true),
            (-4, 6, true), (-4, 6, false), (3, 8, false)
        ];

        var expected = new Dictionary<int, SubscriptionStatus>();

        for (var index = 0; index < cases.Length; index++)
        {
            var (endsInDays, credits, rejected) = cases[index];
            var plan = credits is null ? monthly : pack;
            var member = new Member("Membre", $"N{index:D2}");

            var subscription = new Subscription
            {
                Member = member,
                Plan = plan,
                StartedOn = Today.AddMonths(-2),
                EndsOn = Today.AddDays(endsInDays),
                CreditsRemaining = credits,
                CreditsTotal = credits is null ? null : plan.CreditCount,
                PriceLabel = plan.FormatPriceLabel(),
                Price = plan.Price
            };

            if (rejected)
            {
                subscription.Payments =
                [
                    new Payment
                    {
                        Member = member,
                        Date = Today.AddDays(-10),
                        Label = plan.Name,
                        Amount = plan.Price,
                        Method = PaymentMethod.SepaDirectDebit,
                        Status = PaymentStatus.Rejected
                    }
                ];
            }

            dbContext.Members.Add(member);
            dbContext.Subscriptions.Add(subscription);
        }

        await dbContext.SaveChangesAsync();

        // The label each row carries, from the projected cover.
        var covers = await dbContext.Subscriptions
            .AsNoTracking()
            .Select(subscription => new SubscriptionCoverDto(
                subscription.Id,
                subscription.PlanId,
                subscription.Plan!.Name,
                subscription.Plan.Kind,
                subscription.StartedOn,
                subscription.EndsOn,
                subscription.CreditsRemaining,
                subscription.CreditsTotal,
                subscription.PriceLabel,
                subscription.AutoRenew,
                subscription.Price,
                subscription.Payments!
                    .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m,
                subscription.Payments!.Any(payment =>
                    payment.IsActive && payment.Status != PaymentStatus.Collected)))
            .ToListAsync();

        covers.Count.ShouldBe(cases.Length);

        foreach (var cover in covers)
        {
            expected[cover.SubscriptionId] = SubscriptionStatusRules.Resolve(cover, Today, Horizon);
        }

        // Filtering in SQL selects exactly the rows carrying that label.
        foreach (var status in Enum.GetValues<SubscriptionStatus>())
        {
            var filtered = await dbContext.Subscriptions
                .AsNoTracking()
                .Where(SubscriptionStatusRules.Matches(status, Today, Horizon))
                .Select(subscription => subscription.Id)
                .ToListAsync();

            filtered.OrderBy(id => id).ShouldBe(
                expected.Where(pair => pair.Value == status).Select(pair => pair.Key).OrderBy(id => id),
                $"the SQL filter for {status} must return exactly the subscriptions carrying that label.");
        }

        // And the four together partition the set — no row counted twice, none lost.
        var counted = 0;
        foreach (var status in Enum.GetValues<SubscriptionStatus>())
        {
            counted += await dbContext.Subscriptions
                .CountAsync(SubscriptionStatusRules.Matches(status, Today, Horizon));
        }

        counted.ShouldBe(cases.Length);
    }

    [Fact]
    public void CreditsLabelAndGauges_ShouldReadTheTwoScreensTheyBelongTo()
    {
        var pack = Cover(PlanKind.CreditPack, endsInDays: 40, creditsRemaining: 3);
        pack.CreditsLabel.ShouldBe("3/10");
        pack.CreditsPercent.ShouldBe(30);

        // A recurring plan counts no entries: the members table shows a full bar
        // beside "∞", while the abonnements table fills from the calendar. The
        // two bars mean different things and are computed separately.
        var monthly = Cover(PlanKind.Recurring, endsInDays: 18, startsInDays: -12);
        monthly.CreditsLabel.ShouldBe("∞");
        monthly.CreditsPercent.ShouldBe(100);
        monthly.PeriodPercentRemaining(Today).ShouldBe(60);
    }

    private static SubscriptionCoverDto Cover(
        PlanKind kind,
        int endsInDays,
        int? creditsRemaining = null,
        bool hasOutstandingPayment = false,
        int startsInDays = -30) =>
        new(
            SubscriptionId: 1,
            PlanId: 1,
            PlanName: kind == PlanKind.CreditPack ? "Carte 10 séances" : "Illimité mensuel",
            Kind: kind,
            StartedOn: Today.AddDays(startsInDays),
            EndsOn: Today.AddDays(endsInDays),
            CreditsRemaining: creditsRemaining,
            CreditsTotal: kind == PlanKind.CreditPack ? 10 : null,
            PriceLabel: "49 € / mois",
            AutoRenew: true,
            Price: 49m,
            // Nothing collected when a failure is being simulated, so the
            // shortfall half of the rule holds and the cover really is owing.
            CollectedAmount: hasOutstandingPayment ? 0m : 49m,
            HasFailedPayment: hasOutstandingPayment);
}
