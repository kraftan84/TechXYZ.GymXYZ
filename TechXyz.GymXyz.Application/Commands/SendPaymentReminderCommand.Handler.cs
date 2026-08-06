using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SendPaymentReminderCommandHandler : IRequestHandler<SendPaymentReminderCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<SendPaymentReminderCommand> _validator;

    public SendPaymentReminderCommandHandler(
        IGymDbContext dbContext,
        IValidator<SendPaymentReminderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(SendPaymentReminderCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var subscription = await _dbContext.Subscriptions
            .Include(candidate => candidate.Plan)
            .Include(candidate => candidate.Member)
            .Include(candidate => candidate.Payments)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.SubscriptionId && candidate.IsActive,
                cancellationToken);

        if (subscription?.Member is null || subscription.Plan is null || !subscription.Member.IsActive)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var horizon = SubscriptionStatusRules.HorizonFrom(today);

        var cover = new SubscriptionCoverDto(
            subscription.Id,
            subscription.PlanId,
            subscription.Plan.Name,
            subscription.Plan.Kind,
            subscription.StartedOn,
            subscription.EndsOn,
            subscription.CreditsRemaining,
            subscription.CreditsTotal,
            subscription.PriceLabel,
            subscription.AutoRenew,
            subscription.Price,
            subscription.Payments?
                .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                .Sum(payment => payment.Amount) ?? 0m,
            subscription.Payments?.Any(payment =>
                payment.IsActive && payment.Status != PaymentStatus.Collected) ?? false);

        var status = SubscriptionStatusRules.Resolve(cover, today, horizon);

        // Only the two states a chase is about. Reminding somebody who is paid up
        // and covered for another month is how a gym trains its members to ignore
        // its e-mails.
        if (status is not (SubscriptionStatus.Late or SubscriptionStatus.ExpiringSoon))
        {
            throw ValidationFailures.Refuse(PlanFieldNames.Subscription, PaymentRules.NothingToChase);
        }

        subscription.LastReminderSentOn = today;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
