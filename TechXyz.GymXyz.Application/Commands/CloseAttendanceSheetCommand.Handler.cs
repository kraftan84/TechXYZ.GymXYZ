using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CloseAttendanceSheetCommandHandler : IRequestHandler<CloseAttendanceSheetCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CloseAttendanceSheetCommand> _validator;
    private readonly ICurrentUserService _currentUser;

    public CloseAttendanceSheetCommandHandler(
        IGymDbContext dbContext,
        IValidator<CloseAttendanceSheetCommand> validator,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(CloseAttendanceSheetCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.SessionId && candidate.IsActive,
                cancellationToken);

        if (session is null)
        {
            return false;
        }

        AttendanceCompositionHelper.GuardOwned(session, CoachScope.For(_currentUser));
        AttendanceCompositionHelper.GuardWritable(session);

        var now = DateTime.Now;

        // Validating a class that has not happened would lock a sheet nobody
        // could have pointed.
        if (session.StartsAt > now)
        {
            throw ValidationFailures.Refuse(
                AttendanceFieldNames.Sheet,
                AttendanceRules.SessionNotStarted);
        }

        session.AttendanceClosedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
