using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SendAbsenceReminderCommandValidator : AbstractValidator<SendAbsenceReminderCommand>
{
    public SendAbsenceReminderCommandValidator()
    {
        RuleFor(command => command.MemberIds)
            .NotEmpty().WithMessage(AttendanceRules.NobodyToChase);
    }
}
