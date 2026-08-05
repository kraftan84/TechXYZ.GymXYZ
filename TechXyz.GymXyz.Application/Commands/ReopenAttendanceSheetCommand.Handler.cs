using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class ReopenAttendanceSheetCommandHandler : IRequestHandler<ReopenAttendanceSheetCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<ReopenAttendanceSheetCommand> _validator;
    private readonly ICurrentUserService _currentUser;

    public ReopenAttendanceSheetCommandHandler(
        IGymDbContext dbContext,
        IValidator<ReopenAttendanceSheetCommand> validator,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ReopenAttendanceSheetCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // Asked before the session is even loaded: whether the sheet exists is
        // none of a coach's business either.
        if (!_currentUser.IsInRole(AttendanceRules.ReopenRole))
        {
            throw ValidationFailures.Refuse(
                AttendanceFieldNames.Sheet,
                AttendanceRules.ReopenReserved);
        }

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.SessionId && candidate.IsActive,
                cancellationToken);

        if (session is null)
        {
            return false;
        }

        if (session.AttendanceClosedAt is null)
        {
            throw ValidationFailures.Refuse(
                AttendanceFieldNames.Sheet,
                AttendanceRules.SheetNotClosed);
        }

        session.AttendanceClosedAt = null;
        session.AttendanceReopenedBy = _currentUser.UserName;
        session.AttendanceReopenedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
