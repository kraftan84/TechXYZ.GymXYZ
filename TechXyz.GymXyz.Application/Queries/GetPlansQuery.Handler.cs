using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetPlansQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _dbContext.Plans
            .AsNoTracking()
            .Where(plan => plan.IsActive)
            .OrderBy(plan => plan.Rank)
            .ThenBy(plan => plan.Id)
            .Select(plan => new PlanDto(
                plan.Id,
                plan.Name,
                plan.ShortName,
                plan.Price,
                plan.Unit,
                plan.Kind,
                plan.CreditCount,
                plan.ValidityMonths,
                plan.BillingLabel,
                plan.Description,
                plan.Tone,
                plan.IsFeatured,
                plan.Rank,
                // "64 membres" on the card: people covered by the plan right
                // now, not people who ever bought it.
                plan.Subscriptions!.Count(subscription =>
                    subscription.IsActive &&
                    subscription.StartedOn <= today &&
                    subscription.EndsOn >= today)))
            .ToListAsync(cancellationToken);
    }
}
