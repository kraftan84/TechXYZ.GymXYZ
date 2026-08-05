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
        var subscription = new Subscription
        {
            Member = member,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            NumberOfSessions = 10
        };

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
