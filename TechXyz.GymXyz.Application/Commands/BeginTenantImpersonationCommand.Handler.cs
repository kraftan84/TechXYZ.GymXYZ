using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class BeginTenantImpersonationCommandHandler
    : IRequestHandler<BeginTenantImpersonationCommand, TenantImpersonationDto?>
{
    private readonly IGymDbContext _dbContext;

    public BeginTenantImpersonationCommandHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantImpersonationDto?> Handle(
        BeginTenantImpersonationCommand request,
        CancellationToken cancellationToken)
    {
        // Tenants sit above the global filter, so this reads a customer the
        // caller does not inhabit — which is the whole point of the console.
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.TenantId && candidate.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        // An admin who jumps straight from one customer to another never passes
        // through the exit. Close whatever is still open for them first, so the
        // trail reads as a sequence of visits rather than a pile of open ones.
        await CloseOpenVisitsAsync(request.AdminUserId, cancellationToken);

        var visit = new TenantImpersonation
        {
            AdminUserId = request.AdminUserId,
            AdminEmail = request.AdminEmail,
            TenantId = tenant.Id,
            StartedAt = DateTime.UtcNow
        };

        _dbContext.TenantImpersonations.Add(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TenantImpersonationDto(visit.Id, tenant.Id, tenant.Slug, tenant.DisplayName);
    }

    private async Task CloseOpenVisitsAsync(string adminUserId, CancellationToken cancellationToken)
    {
        // Loaded and mutated rather than ExecuteUpdateAsync: the latter is not
        // supported by the InMemory provider the tests run on.
        var open = await _dbContext.TenantImpersonations
            .Where(visit => visit.AdminUserId == adminUserId && visit.EndedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var visit in open)
        {
            visit.EndedAt = DateTime.UtcNow;
        }
    }
}
