using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdatePlanCommand> _validator;

    public UpdatePlanCommandHandler(IGymDbContext dbContext, IValidator<UpdatePlanCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var plan = await _dbContext.Plans
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);

        if (plan is null)
        {
            return false;
        }

        if (plan.Kind == PlanKind.CreditPack && request.CreditCount is null or <= 0)
        {
            throw ValidationFailures.Refuse(PlanFieldNames.CreditCount, PlanRules.CreditCountRequired);
        }

        plan.Name = request.Name;
        plan.ShortName = request.ShortName;
        plan.Price = request.Price;
        plan.ValidityMonths = request.ValidityMonths;
        plan.Unit = PlanRules.UnitFor(plan.Kind, request.ValidityMonths);
        plan.BillingLabel = PlanRules.BillingLabelFor(plan.Kind, request.HasCommitment, request.ValidityMonths);
        plan.Description = request.Description;
        plan.CreditCount = plan.Kind == PlanKind.CreditPack ? request.CreditCount : null;

        if (request.IsFeatured && !plan.IsFeatured)
        {
            // One card carries the brand rule. Promoting this one demotes the
            // other, or the grid would show two "mises en avant" and neither
            // would read as a choice.
            //
            // Loaded and set rather than an ExecuteUpdate: a gym has a handful
            // of formules, and the set-based version does not translate on every
            // provider the tests run against.
            var previouslyFeatured = await _dbContext.Plans
                .Where(candidate => candidate.Id != plan.Id && candidate.IsFeatured)
                .ToListAsync(cancellationToken);

            foreach (var demoted in previouslyFeatured)
            {
                demoted.IsFeatured = false;
            }
        }

        plan.IsFeatured = request.IsFeatured;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
