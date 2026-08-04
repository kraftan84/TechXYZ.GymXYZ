using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetTenantBrandQueryHandler : IRequestHandler<GetTenantBrandQuery, TenantBrandDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetTenantBrandQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantBrandDto?> Handle(GetTenantBrandQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive && tenant.Slug == request.Slug)
            .SelectTenantBrandDto()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
