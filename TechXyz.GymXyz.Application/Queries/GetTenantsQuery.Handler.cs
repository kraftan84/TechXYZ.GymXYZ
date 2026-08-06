using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetTenantsQueryHandler
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantSummaryDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetTenantsQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TenantSummaryDto>> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken)
    {
        // Two queries, not one per customer: the counts come back grouped by
        // tenant in a single round trip. See TenantMemberCounter for why that one
        // lifts the tenant filter and why nothing else may.
        var counts = await _dbContext.CountActiveByTenantAsync(cancellationToken);

        var tenants = await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive)
            .OrderBy(tenant => tenant.DisplayName)
            .Select(tenant => new
            {
                tenant.Id,
                tenant.Slug,
                tenant.DisplayName,
                tenant.ThemeKey,
                tenant.Baseline,
                tenant.LogoPath,
                tenant.CircleLogo,
                tenant.City,
                tenant.AreaLabel,
                tenant.IsSolo,
                tenant.GymPlan,
                tenant.PlanPrice,
                tenant.PlanRenewalDate,
                tenant.PlanMemberCap
            })
            .ToListAsync(cancellationToken);

        return tenants
            .Select(tenant => new TenantSummaryDto(
                tenant.Id,
                tenant.Slug,
                tenant.DisplayName,
                tenant.ThemeKey,
                tenant.Baseline,
                tenant.LogoPath,
                tenant.CircleLogo,
                tenant.City,
                tenant.AreaLabel,
                tenant.IsSolo,
                tenant.GymPlan,
                tenant.PlanPrice,
                tenant.PlanRenewalDate,
                tenant.PlanMemberCap,
                counts.CountFor(tenant.Id)))
            .ToList();
    }
}
