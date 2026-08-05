using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateSubscriptionCommand> _validator;

    public CreateSubscriptionCommandHandler(IGymDbContext dbContext, IValidator<CreateSubscriptionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var member = await _dbContext.Members
            .FirstOrDefaultAsync(candidate => candidate.Id == request.MemberId && candidate.IsActive, cancellationToken);
        if (member is null)
        {
            throw new ValidationException("Membre introuvable.");
        }

        var subscription = new Subscription
        {
            Member = member,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NumberOfSessions = request.NumberOfSessions
        };

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
