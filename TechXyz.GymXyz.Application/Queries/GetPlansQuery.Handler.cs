using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
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
            .SelectPlanDto(today)
            .ToListAsync(cancellationToken);
    }
}
