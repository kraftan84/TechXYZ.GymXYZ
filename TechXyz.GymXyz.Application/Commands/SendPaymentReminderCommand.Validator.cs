using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SendPaymentReminderCommandValidator : AbstractValidator<SendPaymentReminderCommand>
{
    public SendPaymentReminderCommandValidator()
    {
        RuleFor(command => command.SubscriptionId).GreaterThan(0);
    }
}
