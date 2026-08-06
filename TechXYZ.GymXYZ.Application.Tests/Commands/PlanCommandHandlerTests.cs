using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Creating, editing and retiring a formule. The rule underneath all three: a
/// plan is an offer, and editing an offer must never rewrite what somebody has
/// already bought.
/// </summary>
public class PlanCommandHandlerTests
{
    [Fact]
    public async Task Create_ShouldDeriveTheUnitAndTheEngagementWording()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Create_ShouldDeriveTheUnitAndTheEngagementWording));

        var id = await Create(dbContext).Handle(
            new CreatePlanCommand("Illimité annuel", "Annuel", 490m, PlanKind.Recurring,
                validityMonths: 12, creditCount: null, description: null, hasCommitment: true),
            CancellationToken.None);

        var plan = dbContext.Plans.Single(candidate => candidate.Id == id);
        plan.Unit.ShouldBe("€ / an");
        plan.BillingLabel.ShouldBe("Engagement 12 mois");
        plan.CreditCount.ShouldBeNull();
    }

    [Fact]
    public async Task Create_ShouldLabelAPackAsASinglePurchase()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Create_ShouldLabelAPackAsASinglePurchase));

        var id = await Create(dbContext).Handle(
            new CreatePlanCommand("Carte 5 séances", null, 65m, PlanKind.CreditPack,
                validityMonths: 3, creditCount: 5, description: null, hasCommitment: true),
            CancellationToken.None);

        var plan = dbContext.Plans.Single(candidate => candidate.Id == id);

        // A pack is neither engaged nor not — it is bought once, and the
        // engagement switch does not apply to it.
        plan.Unit.ShouldBe("€ / carte");
        plan.BillingLabel.ShouldBe("Paiement unique");
        plan.CreditCount.ShouldBe(5);

        // No short name given: the full one stands in rather than leaving an
        // empty cell on every attendance sheet.
        plan.ShortName.ShouldBe("Carte 5 séances");
    }

    [Fact]
    public async Task Create_ShouldRefuseAPackWithNoEntries()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Create_ShouldRefuseAPackWithNoEntries));

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Create(dbContext).Handle(
                new CreatePlanCommand("Carte vide", null, 60m, PlanKind.CreditPack,
                    validityMonths: 3, creditCount: 0, description: null, hasCommitment: false),
                CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(PlanRules.CreditCountRequired);
    }

    [Fact]
    public async Task Update_ShouldNotTouchWhatIsAlreadySold()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Update_ShouldNotTouchWhatIsAlreadySold));

        var plan = TestPlans.Monthly();
        dbContext.Plans.Add(plan);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var sold = new Subscription
        {
            Member = new Member("Laetitia", "Moriceau"),
            Plan = plan,
            StartedOn = today.AddDays(-12),
            EndsOn = today.AddDays(18),
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };
        dbContext.Subscriptions.Add(sold);
        await dbContext.SaveChangesAsync();

        var updated = await Update(dbContext).Handle(
            new UpdatePlanCommand(plan.Id, "Illimité mensuel", "Illimité", 59m,
                validityMonths: 1, creditCount: null, description: "Plus cher",
                hasCommitment: false, isFeatured: true),
            CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Plans.Single().Price.ShouldBe(59m);

        // The member on it keeps every figure of the day they signed.
        sold.Price.ShouldBe(49m);
        sold.MonthlyPrice.ShouldBe(49m);
        sold.PriceLabel.ShouldBe("49 € / mois");
    }

    [Fact]
    public async Task Update_ShouldLeaveOnlyOneCardFeatured()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Update_ShouldLeaveOnlyOneCardFeatured));

        var featured = TestPlans.Monthly();
        var other = TestPlans.Pack();
        dbContext.Plans.AddRange(featured, other);
        await dbContext.SaveChangesAsync();

        featured.IsFeatured.ShouldBeTrue();

        await Update(dbContext).Handle(
            new UpdatePlanCommand(other.Id, other.Name, other.ShortName, other.Price,
                other.ValidityMonths, other.CreditCount, other.Description,
                hasCommitment: false, isFeatured: true),
            CancellationToken.None);

        // Two cards wearing the brand rule would mean neither reads as a choice.
        dbContext.Plans.Count(plan => plan.IsFeatured).ShouldBe(1);
        dbContext.Plans.Single(plan => plan.IsFeatured).Id.ShouldBe(other.Id);
    }

    [Fact]
    public async Task Delete_ShouldTakeThePlanOffSaleWithoutTouchingItsSubscriptions()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Delete_ShouldTakeThePlanOffSaleWithoutTouchingItsSubscriptions));

        var plan = TestPlans.Monthly();
        dbContext.Plans.Add(plan);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var sold = new Subscription
        {
            Member = new Member("Sarah", "Cohen"),
            Plan = plan,
            StartedOn = today.AddDays(-3),
            EndsOn = today.AddDays(27),
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };
        dbContext.Subscriptions.Add(sold);
        await dbContext.SaveChangesAsync();

        var deleted = await new DeletePlanCommandHandler(dbContext, new DeletePlanCommandValidator())
            .Handle(new DeletePlanCommand(plan.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        plan.IsActive.ShouldBeFalse();

        // "Retirer de la vente" means nobody new can buy it — not that the
        // people who did lose what they paid for.
        plan.IsFeatured.ShouldBeFalse();
        sold.IsActive.ShouldBeTrue();
        sold.EndsOn.ShouldBe(today.AddDays(27));
    }

    private static CreatePlanCommandHandler Create(GymDbContext dbContext) =>
        new(dbContext, new CreatePlanCommandValidator());

    private static UpdatePlanCommandHandler Update(GymDbContext dbContext) =>
        new(dbContext, new UpdatePlanCommandValidator());
}
