using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(command => command.SubscriptionId).GreaterThan(0);
    }
}
