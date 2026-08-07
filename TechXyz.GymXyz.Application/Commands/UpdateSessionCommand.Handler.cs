using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSessionCommandHandler : IRequestHandler<UpdateSessionCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateSessionCommand> _validator;
    private readonly ICurrentUserService _currentUser;

    public UpdateSessionCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdateSessionCommand> validator,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
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

        // Both ends of the move: the session has to be theirs to start with, and
        // it has to stay theirs. Handing a class to a colleague is rostering, and
        // a coach who did it would then be unable to undo it.
        var scope = CoachScope.For(_currentUser);

        if (!scope.Covers(session) || !scope.CoversCoach(request.CoachId))
        {
            throw ValidationFailures.Refuse(SessionFieldNames.Coach, CoachScope.NotYourSession);
        }

        var location = await SessionCompositionHelper.LoadLocationAsync(
            _dbContext, request.LocationId, cancellationToken);
        var coach = await SessionCompositionHelper.LoadCoachAsync(
            _dbContext, request.CoachId, cancellationToken);

        var capacity = request.Capacity ?? session.Capacity;
        SessionCompositionHelper.EnsureFitsInLocation(location, capacity);

        var registered = await _dbContext.Registrations
            .CountAsync(
                seat => seat.SessionId == session.Id && seat.IsActive && !seat.IsWaitlisted,
                cancellationToken);

        if (capacity < registered)
        {
            throw new ValidationException([
                new ValidationFailure(
                    SessionFieldNames.Capacity,
                    $"La séance compte déjà {registered} inscrits : la capacité ne peut pas descendre en dessous.")
            ]);
        }

        var affected = await LoadScopeAsync(session, request.Scope, cancellationToken);

        // Every occurrence keeps its own distance from the one being edited, so
        // a series stays a series instead of collapsing onto a single date.
        var shift = request.StartsAt - session.StartsAt;
        var duration = session.EndsAt - session.StartsAt;

        var slots = affected
            .Select(candidate => (
                StartsAt: candidate.StartsAt + shift,
                EndsAt: candidate.StartsAt + shift + duration))
            .ToList();

        await SessionCompositionHelper.EnsureSlotsAreFreeAsync(
            _dbContext,
            slots,
            location.Id,
            coach?.Id,
            affected.Select(candidate => candidate.Id).ToList(),
            cancellationToken);

        foreach (var candidate in affected)
        {
            candidate.StartsAt += shift;
            candidate.EndsAt = candidate.StartsAt + duration;
            candidate.LocationId = location.Id;
            candidate.CoachId = coach?.Id;
            candidate.Capacity = capacity;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// The occurrences the edit reaches: this one, plus the later ones of the
    /// same series when asked. A session without a series id is on its own
    /// whatever the scope says.
    /// </summary>
    private async Task<List<Session>> LoadScopeAsync(
        Session session,
        SessionEditScope scope,
        CancellationToken cancellationToken)
    {
        if (scope == SessionEditScope.ThisOne || session.SeriesId is not { } seriesId)
        {
            return [session];
        }

        return await _dbContext.Sessions
            .Where(candidate =>
                candidate.IsActive &&
                candidate.SeriesId == seriesId &&
                candidate.StartsAt >= session.StartsAt &&
                candidate.Status != SessionStatus.Cancelled)
            .OrderBy(candidate => candidate.StartsAt)
            .ToListAsync(cancellationToken);
    }
}
