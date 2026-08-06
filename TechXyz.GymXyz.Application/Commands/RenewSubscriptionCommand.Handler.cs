using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, int?>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<RenewSubscriptionCommand> _validator;

    public RenewSubscriptionCommandHandler(
        IGymDbContext dbContext,
        IValidator<RenewSubscriptionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int?> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var expiring = await _dbContext.Subscriptions
            .Include(candidate => candidate.Plan)
            .Include(candidate => candidate.Member)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.SubscriptionId && candidate.IsActive,
                cancellationToken);

        if (expiring?.Plan is null || expiring.Member is null || !expiring.Member.IsActive)
        {
            return null;
        }

        if (!expiring.Plan.IsActive)
        {
            throw new ValidationException(
                "Cette formule n'est plus proposée : choisissez-en une autre pour ce membre.");
        }

        // A renewal takes over the day after the cover it follows — unless that
        // cover lapsed a while ago, in which case starting it in the past would
        // sell the member weeks they cannot use.
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startedOn = expiring.EndsOn < today ? today : expiring.EndsOn.AddDays(1);

        var renewal = SubscriptionFactory.Create(
            expiring.Plan,
            expiring.MemberId,
            startedOn,
            expiring.AutoRenew);

        _dbContext.Subscriptions.Add(renewal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return renewal.Id;
    }
}
