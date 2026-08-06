using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Selling and renewing. Both go through the plan, and both snapshot what they
/// sold — the price and the entry count are copied onto the subscription so that
/// changing a formule tomorrow cannot rewrite what somebody bought today.
/// </summary>
public class SubscriptionCommandHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task Assign_ShouldTakeEverythingFromThePlan()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Assign_ShouldTakeEverythingFromThePlan));
        var (member, plan) = Seed(dbContext, TestPlans.Pack());
        await dbContext.SaveChangesAsync();

        var id = await Assign(dbContext).Handle(
            new AssignSubscriptionCommand(member.Id, plan.Id, Today, autoRenew: false),
            CancellationToken.None);

        var subscription = dbContext.Subscriptions.Single(candidate => candidate.Id == id);
        subscription.StartedOn.ShouldBe(Today);

        // Four months of validity, ending the day before the anniversary: no day
        // covered twice, none missed when the next one starts.
        subscription.EndsOn.ShouldBe(Today.AddMonths(4).AddDays(-1));
        subscription.CreditsRemaining.ShouldBe(10);
        subscription.CreditsTotal.ShouldBe(10);
        subscription.PriceLabel.ShouldBe("120 € / carte");
    }

    [Fact]
    public async Task Assign_ShouldLeaveARecurringPlanWithoutCredits()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Assign_ShouldLeaveARecurringPlanWithoutCredits));
        var (member, plan) = Seed(dbContext, TestPlans.Monthly());
        await dbContext.SaveChangesAsync();

        var id = await Assign(dbContext).Handle(
            new AssignSubscriptionCommand(member.Id, plan.Id, Today, autoRenew: true),
            CancellationToken.None);

        var subscription = dbContext.Subscriptions.Single(candidate => candidate.Id == id);
        subscription.CreditsRemaining.ShouldBeNull();
        subscription.CreditsTotal.ShouldBeNull();
        subscription.AutoRenew.ShouldBeTrue();
        subscription.EndsOn.ShouldBe(Today.AddMonths(1).AddDays(-1));
    }

    [Fact]
    public async Task Assign_ShouldRefuseAFormuleThatIsNotOnSale()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Assign_ShouldRefuseAFormuleThatIsNotOnSale));
        var (member, plan) = Seed(dbContext, TestPlans.Monthly());
        plan.IsActive = false;
        await dbContext.SaveChangesAsync();

        await Should.ThrowAsync<ValidationException>(async () =>
            await Assign(dbContext).Handle(
                new AssignSubscriptionCommand(member.Id, plan.Id, Today, autoRenew: true),
                CancellationToken.None));
    }

    [Fact]
    public async Task Renew_ShouldStartTheDayAfterTheCoverItFollows()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Renew_ShouldStartTheDayAfterTheCoverItFollows));
        var (member, plan) = Seed(dbContext, TestPlans.Monthly());
        var current = Cover(member, plan, startsInDays: -25, endsInDays: 5);
        dbContext.Subscriptions.Add(current);
        await dbContext.SaveChangesAsync();

        var id = await Renew(dbContext).Handle(
            new RenewSubscriptionCommand(current.Id), CancellationToken.None);

        var renewal = dbContext.Subscriptions.Single(candidate => candidate.Id == id!.Value);
        renewal.StartedOn.ShouldBe(current.EndsOn.AddDays(1));
        renewal.PlanId.ShouldBe(plan.Id);

        // A renewal is a new row. The cover the member had is history, and
        // stretching its end date would erase it.
        current.EndsOn.ShouldBe(Today.AddDays(5));
        dbContext.Subscriptions.Count(candidate => candidate.MemberId == member.Id).ShouldBe(2);
    }

    [Fact]
    public async Task Renew_ShouldStartToday_WhenTheCoverLapsedLongAgo()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Renew_ShouldStartToday_WhenTheCoverLapsedLongAgo));
        var (member, plan) = Seed(dbContext, TestPlans.Monthly());
        var lapsed = Cover(member, plan, startsInDays: -90, endsInDays: -25);
        dbContext.Subscriptions.Add(lapsed);
        await dbContext.SaveChangesAsync();

        var id = await Renew(dbContext).Handle(
            new RenewSubscriptionCommand(lapsed.Id), CancellationToken.None);

        // Backdating it would sell the member three weeks they cannot use.
        var renewal = dbContext.Subscriptions.Single(candidate => candidate.Id == id!.Value);
        renewal.StartedOn.ShouldBe(Today);
    }

    [Fact]
    public async Task Renew_ShouldRefuseAFormuleThatIsNoLongerProposed()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Renew_ShouldRefuseAFormuleThatIsNoLongerProposed));
        var (member, plan) = Seed(dbContext, TestPlans.Monthly());
        var current = Cover(member, plan, startsInDays: -25, endsInDays: 5);
        dbContext.Subscriptions.Add(current);
        await dbContext.SaveChangesAsync();

        plan.IsActive = false;
        await dbContext.SaveChangesAsync();

        // Renewing onto a retired plan would put a member back on something the
        // gym has stopped selling — the drawer has to offer a different one.
        await Should.ThrowAsync<ValidationException>(async () =>
            await Renew(dbContext).Handle(new RenewSubscriptionCommand(current.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Renew_ShouldReturnNull_WhenTheSubscriptionIsGone()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Renew_ShouldReturnNull_WhenTheSubscriptionIsGone));

        var renewed = await Renew(dbContext).Handle(new RenewSubscriptionCommand(999), CancellationToken.None);

        renewed.ShouldBeNull();
    }

    private static AssignSubscriptionCommandHandler Assign(GymDbContext dbContext) =>
        new(dbContext, new AssignSubscriptionCommandValidator());

    private static RenewSubscriptionCommandHandler Renew(GymDbContext dbContext) =>
        new(dbContext, new RenewSubscriptionCommandValidator());

    private static (Member Member, Plan Plan) Seed(GymDbContext dbContext, Plan plan)
    {
        var member = new Member("Camille", "Durand");
        dbContext.Members.Add(member);
        dbContext.Plans.Add(plan);

        return (member, plan);
    }

    private static Subscription Cover(Member member, Plan plan, int startsInDays, int endsInDays) => new()
    {
        Member = member,
        Plan = plan,
        StartedOn = Today.AddDays(startsInDays),
        EndsOn = Today.AddDays(endsInDays),
        CreditsRemaining = plan.IsCredited ? plan.CreditCount : null,
        CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
        PriceLabel = plan.FormatPriceLabel()
    };
}
