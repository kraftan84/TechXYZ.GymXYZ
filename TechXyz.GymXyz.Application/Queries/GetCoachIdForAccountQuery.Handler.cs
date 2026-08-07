using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachIdForAccountQueryHandler
    : IRequestHandler<GetCoachIdForAccountQuery, int?>
{
    private readonly IGymDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetCoachIdForAccountQueryHandler(IGymDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<int?> Handle(GetCoachIdForAccountQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return null;
        }

        // Scoped explicitly: the global filter reads the ambient tenant, and at
        // sign-in that is still the host's guess rather than this account's gym.
        using (_tenantContext.UseTenant(request.TenantId))
        {
            return await _dbContext.Coaches
                .AsNoTracking()
                .Where(coach => coach.IsActive && coach.UserId == request.UserId)
                .Select(coach => (int?)coach.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
