using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class MarkWholeSheetCommandHandler : IRequestHandler<MarkWholeSheetCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<MarkWholeSheetCommand> _validator;

    public MarkWholeSheetCommandHandler(
        IGymDbContext dbContext,
        IValidator<MarkWholeSheetCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(MarkWholeSheetCommand request, CancellationToken cancellationToken)
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

        AttendanceCompositionHelper.GuardWritable(session);

        var seats = await _dbContext.Registrations
            .Where(registration =>
                registration.SessionId == session.Id &&
                registration.IsActive &&
                !registration.IsWaitlisted)
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;
        foreach (var seat in seats)
        {
            AttendanceCompositionHelper.Apply(seat, request.Status, now);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
