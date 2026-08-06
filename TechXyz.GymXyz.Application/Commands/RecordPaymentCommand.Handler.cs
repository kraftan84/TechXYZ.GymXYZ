using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<RecordPaymentCommand> _validator;

    public RecordPaymentCommandHandler(IGymDbContext dbContext, IValidator<RecordPaymentCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var memberExists = await _dbContext.Members
            .AnyAsync(candidate => candidate.Id == request.MemberId && candidate.IsActive, cancellationToken);

        if (!memberExists)
        {
            throw ValidationFailures.Refuse(PlanFieldNames.Member, PaymentRules.MemberNotFound);
        }

        var label = "Encaissement";

        if (request.SubscriptionId is { } subscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .Include(candidate => candidate.Plan)
                .FirstOrDefaultAsync(
                    candidate => candidate.Id == subscriptionId && candidate.IsActive,
                    cancellationToken);

            if (subscription is null)
            {
                throw ValidationFailures.Refuse(
                    PlanFieldNames.Subscription, PaymentRules.SubscriptionNotFound);
            }

            // Attaching a payment to somebody else's cover would move money
            // between two members' accounts and make one of them read paid up.
            if (subscription.MemberId != request.MemberId)
            {
                throw ValidationFailures.Refuse(
                    PlanFieldNames.Subscription, PaymentRules.SubscriptionNotOwned);
            }

            // The plan name as it stands now, copied onto the row: the list is a
            // history, and a formule renamed next year must not relabel what was
            // paid for this year.
            label = subscription.Plan?.Name ?? label;
        }

        var payment = new Payment
        {
            MemberId = request.MemberId,
            SubscriptionId = request.SubscriptionId,
            Date = request.Date,
            Label = label,
            Amount = request.Amount,
            Method = request.Method,
            Status = request.Status
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
