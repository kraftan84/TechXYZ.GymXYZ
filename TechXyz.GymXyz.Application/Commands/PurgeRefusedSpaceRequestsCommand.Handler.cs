using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class PurgeRefusedSpaceRequestsCommandHandler
    : IRequestHandler<PurgeRefusedSpaceRequestsCommand, int>
{
    private readonly IGymDbContext _dbContext;

    public PurgeRefusedSpaceRequestsCommandHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(
        PurgeRefusedSpaceRequestsCommand request,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - PurgeRefusedSpaceRequestsCommand.RetentionAfterRefusal;

        // A refusal with no date is not swept. It cannot be: there is nothing to
        // count three months from. The console sets both together when it refuses,
        // and a row that somehow lost its date is a bug to find rather than a row
        // to delete on a guess.
        var expired = await _dbContext.SpaceRequests
            .Where(candidate => candidate.Status == SpaceRequestStatus.Refused
                                && candidate.RefusedOn != null
                                && candidate.RefusedOn < cutoff)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return 0;
        }

        // Deleted, not deactivated. IsActive is how this codebase retires rows it
        // still wants; what was promised here is deletion, and a soft-deleted
        // dossier still holds the name, address and telephone number of somebody
        // who was told they were gone. Activities and notes go with it by cascade.
        _dbContext.SpaceRequests.RemoveRange(expired);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }
}
