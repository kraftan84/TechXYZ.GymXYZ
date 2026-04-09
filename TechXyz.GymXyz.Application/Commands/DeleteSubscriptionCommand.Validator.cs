using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSubscriptionCommandValidator : AbstractValidator<DeleteSubscriptionCommand>
{
    public DeleteSubscriptionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
