using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<MarkAttendanceCommand> _validator;
    private readonly ICurrentUserService _currentUser;

    public MarkAttendanceCommandHandler(
        IGymDbContext dbContext,
        IValidator<MarkAttendanceCommand> validator,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var registration = await _dbContext.Registrations
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.RegistrationId && candidate.IsActive,
                cancellationToken);

        if (registration is null)
        {
            return false;
        }

        var session = await AttendanceCompositionHelper.LoadSessionAsync(
            _dbContext,
            registration.SessionId,
            cancellationToken);

        AttendanceCompositionHelper.GuardOwned(session, CoachScope.For(_currentUser));
        AttendanceCompositionHelper.GuardWritable(session);

        // The cover is read on the day of the session, not on today: a sheet
        // corrected a week later must spend the pack that was running then.
        var ledger = await CreditLedger.LoadAsync(
            _dbContext,
            [registration],
            DateOnly.FromDateTime(session.StartsAt),
            cancellationToken);

        AttendanceCompositionHelper.Apply(registration, request.Status, DateTime.Now, ledger);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
