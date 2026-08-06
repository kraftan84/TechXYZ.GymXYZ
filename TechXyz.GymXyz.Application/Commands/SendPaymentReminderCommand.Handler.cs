using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SendPaymentReminderCommandHandler
    : IRequestHandler<SendPaymentReminderCommand, NotificationOutcomeDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<SendPaymentReminderCommand> _validator;

    public SendPaymentReminderCommandHandler(
        IGymDbContext dbContext,
        IEmailSender emailSender,
        ITenantContext tenantContext,
        IValidator<SendPaymentReminderCommand> validator)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tenantContext = tenantContext;
        _validator = validator;
    }

    public async Task<NotificationOutcomeDto> Handle(
        SendPaymentReminderCommand request,
        CancellationToken cancellationToken)
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
            return NotificationOutcomeDto.NotFound;
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

        // Stamped and committed before anything leaves: the decision to chase is
        // the gym's, and it survives a mail server that is not answering.
        subscription.LastReminderSentOn = today;
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!await NotificationPolicy.AllowsEmailAsync(
                _dbContext, NotificationKey.RenewalReminder, cancellationToken))
        {
            return NotificationOutcomeDto.Suppressed;
        }

        var member = subscription.Member;
        if (string.IsNullOrWhiteSpace(member.Email))
        {
            // Nothing to send to. Not a failure of ours, and not something a
            // retry would fix — the member's record is where that is fixed.
            return NotificationOutcomeDto.SavedOnly;
        }

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == _tenantContext.Current, cancellationToken);

        var message = NotificationMessages.RenewalReminder(
            tenant?.DisplayName ?? string.Empty,
            member.FirstName,
            member.Email,
            $"{member.FirstName} {member.LastName}",
            subscription.Plan.Name,
            subscription.EndsOn,
            status == SubscriptionStatus.Late) with
        {
            FromName = tenant?.DisplayName,
            ReplyToAddress = tenant?.Email,
            ReplyToName = tenant?.DisplayName
        };

        var delivery = await _emailSender.SendAsync(message, cancellationToken);

        return delivery.IsSent
            ? NotificationOutcomeDto.Delivered(1, 0)
            : NotificationOutcomeDto.Delivered(0, 1);
    }
}
