using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class EndTenantImpersonationCommandHandler
    : IRequestHandler<EndTenantImpersonationCommand, bool>
{
    private readonly IGymDbContext _dbContext;

    public EndTenantImpersonationCommandHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        EndTenantImpersonationCommand request,
        CancellationToken cancellationToken)
    {
        var visit = await _dbContext.TenantImpersonations
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.VisitId
                             && candidate.AdminUserId == request.AdminUserId
                             && candidate.EndedAt == null,
                cancellationToken);

        if (visit is null)
        {
            return false;
        }

        visit.EndedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
