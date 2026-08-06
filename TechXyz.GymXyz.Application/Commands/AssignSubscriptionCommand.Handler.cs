using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class AssignSubscriptionCommandHandler : IRequestHandler<AssignSubscriptionCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<AssignSubscriptionCommand> _validator;

    public AssignSubscriptionCommandHandler(
        IGymDbContext dbContext,
        IValidator<AssignSubscriptionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(AssignSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var memberExists = await _dbContext.Members
            .AnyAsync(candidate => candidate.Id == request.MemberId && candidate.IsActive, cancellationToken);
        if (!memberExists)
        {
            throw new ValidationException("Membre introuvable.");
        }

        var plan = await _dbContext.Plans
            .FirstOrDefaultAsync(candidate => candidate.Id == request.PlanId && candidate.IsActive, cancellationToken);
        if (plan is null)
        {
            throw new ValidationException("Formule introuvable.");
        }

        var subscription = SubscriptionFactory.Create(plan, request.MemberId, request.StartedOn, request.AutoRenew);

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
