using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetTenantBrandByIdQueryHandler : IRequestHandler<GetTenantBrandByIdQuery, TenantBrandDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetTenantBrandByIdQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantBrandDto?> Handle(GetTenantBrandByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive && tenant.Id == request.TenantId)
            .SelectTenantBrandDto()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
