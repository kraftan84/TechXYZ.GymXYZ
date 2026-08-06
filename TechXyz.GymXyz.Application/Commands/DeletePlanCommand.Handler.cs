using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeletePlanCommand> _validator;

    public DeletePlanCommandHandler(IGymDbContext dbContext, IValidator<DeletePlanCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var plan = await _dbContext.Plans
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);

        if (plan is null)
        {
            return false;
        }

        // The subscriptions on it are left exactly as they are. Cascading would
        // wipe the covers people paid for, and that is the opposite of what
        // "retirer de la vente" means.
        plan.IsActive = false;

        // A retired plan cannot be the featured one: the grid would carry a
        // brand rule down a card nobody can see.
        plan.IsFeatured = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
