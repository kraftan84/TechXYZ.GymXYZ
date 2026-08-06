using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The standing is written twice: <c>Resolve</c> produces the label, <c>Matches</c>
/// filters in SQL. These tests pin them to each other — on the relational
/// provider, so a rule that cannot be translated fails here rather than in
/// production.
/// </summary>
public class MemberStatusRulesTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Horizon = MemberStatusRules.HorizonFrom(Today);

    [Fact]
    public void HorizonFrom_ShouldSitSevenDaysAhead()
    {
        MemberStatusRules.ExpiringSoonWithinDays.ShouldBe(7);
        Horizon.ShouldBe(Today.AddDays(7));
    }

    [Theory]
    [InlineData(SubscriptionStatus.Active, MemberStatus.Active)]
    [InlineData(SubscriptionStatus.ExpiringSoon, MemberStatus.ExpiringSoon)]
    [InlineData(SubscriptionStatus.Late, MemberStatus.Inactive)]
    [InlineData(SubscriptionStatus.Ended, MemberStatus.Inactive)]
    public void Project_ShouldFoldLateAndEndedOntoInactive(SubscriptionStatus status, MemberStatus expected)
    {
        // The decision of lot 7: the members table keeps its three chips. Théo
        // Garnier reads "En retard" on the abonnements screen and "Inactif"
        // here, which is what the prototype draws.
        MemberStatusRules.Project(status).ShouldBe(expected);
    }

    [Fact]
    public void Resolve_ShouldReadNoCoverAsInactive()
    {
        MemberStatusRules.Resolve([], Today, Horizon).ShouldBe(MemberStatus.Inactive);
    }

    [Fact]
    public void Resolve_ShouldTakeTheHealthiestOfSeveralCovers()
    {
        // A pack run dry beside a monthly plan in good standing: the member is
        // active, not late. Somebody who bought two things owns the better one.
        var spentPack = Cover(
            kind: PlanKind.CreditPack, endsInDays: 30, creditsRemaining: 0, creditsTotal: 10,
            hasOutstandingPayment: true);
        var healthy = Cover(kind: PlanKind.Recurring, endsInDays: 25);

        MemberStatusRules.Resolve([spentPack, healthy], Today, Horizon).ShouldBe(MemberStatus.Active);
    }

    [Fact]
    public void Resolve_ShouldIgnoreACoverThatHasNotBegun()
    {
        // A renewal booked for next month is not a standing today.
        var booked = Cover(kind: PlanKind.Recurring, startsInDays: 20, endsInDays: 50);

        MemberStatusRules.Resolve([booked], Today, Horizon).ShouldBe(MemberStatus.Inactive);
    }

    [Fact]
    public async Task Matches_ShouldAgreeWithResolve_OnEveryBoundary_OnSqlite()
    {
        // One member per boundary of the rule: the two edges of the warning
        // window, both sides of "no cover at all", a cover booked ahead, and the
        // two legs a pack adds — nearly spent, and spent with money outstanding.
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var monthly = TestPlans.Monthly();
        var pack = TestPlans.Pack();
        dbContext.Plans.AddRange(monthly, pack);

        int?[] boundaries = [null, -1, 0, 1, 7, 8, 120];
        foreach (var endsInDays in boundaries)
        {
            var member = new Member("Membre", NameFor(endsInDays));

            if (endsInDays is { } days)
            {
                member.Subscriptions =
                [
                    new Subscription
                    {
                        Plan = monthly,
                        StartedOn = Today.AddMonths(-2),
                        EndsOn = Today.AddDays(days),
                        PriceLabel = monthly.FormatPriceLabel()
                    }
                ];
            }

            dbContext.Members.Add(member);
        }

        dbContext.Members.Add(WithPack("PackBas", pack, endsInDays: 60, creditsRemaining: 2));
        dbContext.Members.Add(WithPack("PackVide", pack, endsInDays: 60, creditsRemaining: 0));
        dbContext.Members.Add(WithPack("PackImpaye", pack, endsInDays: -3, creditsRemaining: 4, rejected: true));
        dbContext.Members.Add(new Member("Membre", "Reserve")
        {
            Subscriptions =
            [
                new Subscription
                {
                    Plan = monthly,
                    StartedOn = Today.AddDays(20),
                    EndsOn = Today.AddDays(50),
                    PriceLabel = monthly.FormatPriceLabel()
                }
            ]
        });

        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var everyone = await handler.Handle(new GetMembersQuery { PageSize = 200 }, CancellationToken.None);

        everyone.Items.Count.ShouldBe(boundaries.Length + 4);

        // 1. The label each row carries is Resolve applied to its own covers.
        foreach (var item in everyone.Items)
        {
            item.Status.ShouldBe(MemberStatusRules.Resolve(item.Covers, Today, Horizon));
        }

        // 2. Filtering in SQL selects exactly the rows carrying that label.
        foreach (var status in Enum.GetValues<MemberStatus>())
        {
            var filtered = await handler.Handle(
                new GetMembersQuery { Status = status, PageSize = 200 }, CancellationToken.None);

            filtered.Items.Select(item => item.LastName).OrderBy(name => name)
                .ShouldBe(
                    everyone.Items.Where(item => item.Status == status)
                        .Select(item => item.LastName).OrderBy(name => name),
                    $"the SQL filter for {status} must return exactly the members carrying that label.");
        }

        // 3. The three counts partition the list, no row counted twice or lost.
        everyone.ActiveCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.Active));
        everyone.ExpiringSoonCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.ExpiringSoon));
        everyone.InactiveCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.Inactive));
        (everyone.ActiveCount + everyone.ExpiringSoonCount + everyone.InactiveCount)
            .ShouldBe(everyone.TotalCount);
    }

    private static Member WithPack(
        string lastName,
        Plan pack,
        int endsInDays,
        int creditsRemaining,
        bool rejected = false)
    {
        var subscription = new Subscription
        {
            Plan = pack,
            StartedOn = Today.AddMonths(-2),
            EndsOn = Today.AddDays(endsInDays),
            CreditsRemaining = creditsRemaining,
            CreditsTotal = pack.CreditCount,
            PriceLabel = pack.FormatPriceLabel()
        };

        var member = new Member("Membre", lastName) { Subscriptions = [subscription] };

        if (rejected)
        {
            subscription.Payments =
            [
                new Payment
                {
                    Member = member,
                    Date = Today.AddDays(-10),
                    Label = pack.Name,
                    Amount = pack.Price,
                    Method = PaymentMethod.SepaDirectDebit,
                    Status = PaymentStatus.Rejected
                }
            ];
        }

        return member;
    }

    private static SubscriptionCoverDto Cover(
        PlanKind kind,
        int endsInDays,
        int startsInDays = -30,
        int? creditsRemaining = null,
        int? creditsTotal = null,
        bool hasOutstandingPayment = false) =>
        new(
            SubscriptionId: 1,
            PlanId: 1,
            PlanName: "Formule",
            Kind: kind,
            StartedOn: Today.AddDays(startsInDays),
            EndsOn: Today.AddDays(endsInDays),
            CreditsRemaining: creditsRemaining,
            CreditsTotal: creditsTotal,
            PriceLabel: "49 € / mois",
            AutoRenew: true,
            HasOutstandingPayment: hasOutstandingPayment);

    private static string NameFor(int? endsInDays)
        => endsInDays is { } days ? $"J{days + 100:D3}" : "Aucun";
}
