using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSubscription()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateSubscription));
        var member = new Member("John", "Doe");
        var subscription = new Subscription
        {
            Member = member,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            NumberOfSessions = 10
        };

        dbContext.Members.Add(member);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSubscriptionCommandHandler(dbContext, new UpdateSubscriptionCommandValidator());
        var updated = await handler.Handle(
            new UpdateSubscriptionCommand(
                subscription.Id,
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
                20),
            CancellationToken.None);

        updated.ShouldBeTrue();
        subscription.NumberOfSessions.ShouldBe(20);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenSubscriptionNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenSubscriptionNotFound));
        var handler = new UpdateSubscriptionCommandHandler(dbContext, new UpdateSubscriptionCommandValidator());

        var updated = await handler.Handle(
            new UpdateSubscriptionCommand(
                999,
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                10),
            CancellationToken.None);

        updated.ShouldBeFalse();
    }
}
