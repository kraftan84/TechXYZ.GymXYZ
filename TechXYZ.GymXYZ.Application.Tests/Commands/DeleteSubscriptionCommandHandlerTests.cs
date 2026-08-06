using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteSubscription()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteSubscription));
        var member = new Member("John", "Doe");
        var plan = TestPlans.Pack();
        var subscription = new Subscription
        {
            Member = member,
            Plan = plan,
            StartedOn = DateOnly.FromDateTime(DateTime.Today),
            EndsOn = DateOnly.FromDateTime(DateTime.Today.AddMonths(4)),
            CreditsRemaining = 10,
            CreditsTotal = 10,
            PriceLabel = plan.FormatPriceLabel()
        };

        dbContext.Plans.Add(plan);
        dbContext.Members.Add(member);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteSubscriptionCommandHandler(dbContext, new DeleteSubscriptionCommandValidator());
        var deleted = await handler.Handle(new DeleteSubscriptionCommand(subscription.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        subscription.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenSubscriptionNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenSubscriptionNotFound));
        var handler = new DeleteSubscriptionCommandHandler(dbContext, new DeleteSubscriptionCommandValidator());

        var deleted = await handler.Handle(new DeleteSubscriptionCommand(999), CancellationToken.None);

        deleted.ShouldBeFalse();
    }
}
