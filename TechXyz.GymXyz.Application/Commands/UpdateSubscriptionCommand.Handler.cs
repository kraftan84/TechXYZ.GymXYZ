using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateSubscriptionCommand> _validator;

    public UpdateSubscriptionCommandHandler(IGymDbContext dbContext, IValidator<UpdateSubscriptionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var subscription = await _dbContext.Subscriptions
            .Include(candidate => candidate.Member)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (subscription is null || !subscription.Member.IsActive)
        {
            return false;
        }

        subscription.StartDate = request.StartDate;
        subscription.EndDate = request.EndDate;
        subscription.NumberOfLessons = request.NumberOfLessons;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
