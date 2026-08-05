using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class MarkWholeSheetCommandValidator : AbstractValidator<MarkWholeSheetCommand>
{
    public MarkWholeSheetCommandValidator()
    {
        RuleFor(command => command.SessionId).GreaterThan(0);

        RuleFor(command => command.Status)
            .IsInEnum()
            .WithName(AttendanceFieldNames.Status);
    }
}
