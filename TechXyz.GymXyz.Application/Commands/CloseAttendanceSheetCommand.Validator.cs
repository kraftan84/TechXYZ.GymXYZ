using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CloseAttendanceSheetCommandValidator : AbstractValidator<CloseAttendanceSheetCommand>
{
    public CloseAttendanceSheetCommandValidator()
    {
        RuleFor(command => command.SessionId).GreaterThan(0);
    }
}
