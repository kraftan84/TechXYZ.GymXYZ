using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class ReopenAttendanceSheetCommandValidator : AbstractValidator<ReopenAttendanceSheetCommand>
{
    public ReopenAttendanceSheetCommandValidator()
    {
        RuleFor(command => command.SessionId).GreaterThan(0);
    }
}
