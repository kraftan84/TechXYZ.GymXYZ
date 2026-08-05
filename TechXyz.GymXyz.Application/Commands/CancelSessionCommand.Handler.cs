using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CancelSessionCommand> _validator;

    public CancelSessionCommandHandler(IGymDbContext dbContext, IValidator<CancelSessionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);

        if (session is null)
        {
            return false;
        }

        var reason = AddressHelper.NormalizeOptional(request.Reason);
        var affected = await LoadScopeAsync(session, request.Scope, cancellationToken);

        foreach (var candidate in affected)
        {
            candidate.Status = SessionStatus.Cancelled;
            candidate.CancellationReason = reason;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<List<Session>> LoadScopeAsync(
        Session session,
        SessionEditScope scope,
        CancellationToken cancellationToken)
    {
        if (scope == SessionEditScope.ThisOne || session.SeriesId is not { } seriesId)
        {
            return [session];
        }

        // Only forward: cancelling a series must not reach back and rewrite the
        // occurrences that already ran.
        return await _dbContext.Sessions
            .Where(candidate =>
                candidate.IsActive &&
                candidate.SeriesId == seriesId &&
                candidate.StartsAt >= session.StartsAt)
            .ToListAsync(cancellationToken);
    }
}
