using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSubscription()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateSubscription));
        var member = new Member("John", "Doe");
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new CreateSubscriptionCommandHandler(dbContext, new CreateSubscriptionCommandValidator());
        var subscriptionId = await handler.Handle(
            new CreateSubscriptionCommand(
                member.Id,
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                12),
            CancellationToken.None);

        var subscription = await dbContext.Subscriptions.FindAsync(subscriptionId);
        subscription.ShouldNotBeNull();
        subscription!.NumberOfLessons.ShouldBe(12);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenMemberNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenMemberNotFound));
        var handler = new CreateSubscriptionCommandHandler(dbContext, new CreateSubscriptionCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateSubscriptionCommand(
                999,
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                10),
            CancellationToken.None));
    }
}
