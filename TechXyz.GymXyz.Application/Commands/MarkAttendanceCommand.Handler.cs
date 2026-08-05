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

    public MarkAttendanceCommandHandler(
        IGymDbContext dbContext,
        IValidator<MarkAttendanceCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
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

        AttendanceCompositionHelper.GuardWritable(session);

        AttendanceCompositionHelper.Apply(registration, request.Status, DateTime.Now);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
