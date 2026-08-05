using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);

        RuleFor(command => command.Reason)
            .MaximumLength(SessionRules.MaximumCancellationReasonLength)
            .WithName(SessionFieldNames.CancellationReason);
    }
}
