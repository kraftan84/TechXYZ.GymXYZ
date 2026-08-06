using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreatePlanCommand> _validator;

    public CreatePlanCommandHandler(IGymDbContext dbContext, IValidator<CreatePlanCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // New cards land after the ones already there. Which card is featured is
        // a commercial choice nobody makes by adding one.
        var lastRank = await _dbContext.Plans
            .Where(plan => plan.IsActive)
            .MaxAsync(plan => (int?)plan.Rank, cancellationToken);

        var newPlan = new Plan
        {
            Name = request.Name,
            ShortName = request.ShortName,
            Price = request.Price,
            Unit = PlanRules.UnitFor(request.Kind, request.ValidityMonths),
            Kind = request.Kind,
            CreditCount = request.CreditCount,
            ValidityMonths = request.ValidityMonths,
            BillingLabel = PlanRules.BillingLabelFor(request.Kind, request.HasCommitment, request.ValidityMonths),
            Description = request.Description,
            Tone = "neutral",
            Rank = (lastRank ?? -1) + 1
        };

        _dbContext.Plans.Add(newPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newPlan.Id;
    }
}
