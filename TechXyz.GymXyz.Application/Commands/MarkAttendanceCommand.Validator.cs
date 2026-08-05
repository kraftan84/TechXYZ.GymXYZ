using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(command => command.RegistrationId).GreaterThan(0);

        RuleFor(command => command.Status)
            .IsInEnum()
            .WithName(AttendanceFieldNames.Status);
    }
}
