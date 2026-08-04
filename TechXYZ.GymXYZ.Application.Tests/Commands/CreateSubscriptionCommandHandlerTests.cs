using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSubscription_WithDefaultDates()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateSubscription_WithDefaultDates));
        var member = new Member("John", "Doe");
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var expectedStartDate = DateOnly.FromDateTime(DateTime.Today);
        var expectedEndDate = expectedStartDate.AddYears(1);
        var handler = new CreateSubscriptionCommandHandler(dbContext, new CreateSubscriptionCommandValidator());
        var subscriptionId = await handler.Handle(
            new CreateSubscriptionCommand(
                member.Id,
                null,
                null,
                12),
            CancellationToken.None);

        var subscription = await dbContext.Subscriptions.FindAsync(subscriptionId);
        subscription.ShouldNotBeNull();
        subscription.NumberOfLessons.ShouldBe(12);
        subscription.StartDate.ShouldBe(expectedStartDate);
        subscription.EndDate.ShouldBe(expectedEndDate);
    }

    [Fact]
    public async Task Handle_ShouldPreserveExplicitDates()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldPreserveExplicitDates));
        var member = new Member("John", "Doe");
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var startDate = new DateOnly(2026, 4, 12);
        var endDate = new DateOnly(2026, 10, 12);
        var handler = new CreateSubscriptionCommandHandler(dbContext, new CreateSubscriptionCommandValidator());

        var subscriptionId = await handler.Handle(
            new CreateSubscriptionCommand(member.Id, startDate, endDate, 8),
            CancellationToken.None);

        var subscription = await dbContext.Subscriptions.FindAsync(subscriptionId);
        subscription.ShouldNotBeNull();
        subscription.StartDate.ShouldBe(startDate);
        subscription.EndDate.ShouldBe(endDate);
        subscription.NumberOfLessons.ShouldBe(8);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenMemberNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenMemberNotFound));
        var handler = new CreateSubscriptionCommandHandler(dbContext, new CreateSubscriptionCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateSubscriptionCommand(
                999,
                null,
                null,
                10),
            CancellationToken.None));
    }
}
