using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(command => command.MemberId).GreaterThan(0);

        RuleFor(command => command.Amount)
            .GreaterThan(0).WithMessage(PaymentRules.AmountRequired);

        // A payment recorded for next month is a typo, not a forecast.
        RuleFor(command => command.Date)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
            .WithMessage(PaymentRules.DateInTheFuture);
    }
}
