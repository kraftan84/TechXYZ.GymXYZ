using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSubscriptionCommandHandler : IRequestHandler<DeleteSubscriptionCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteSubscriptionCommand> _validator;

    public DeleteSubscriptionCommandHandler(IGymDbContext dbContext, IValidator<DeleteSubscriptionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var subscription = await _dbContext.Subscriptions
            .Include(candidate => candidate.Member)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (subscription is null || !subscription.Member.IsActive)
        {
            return false;
        }

        subscription.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
