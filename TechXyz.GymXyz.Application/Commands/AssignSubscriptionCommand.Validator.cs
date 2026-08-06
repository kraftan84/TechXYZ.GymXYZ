using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class AssignSubscriptionCommandValidator : AbstractValidator<AssignSubscriptionCommand>
{
    public AssignSubscriptionCommandValidator()
    {
        RuleFor(command => command.MemberId).GreaterThan(0);
        RuleFor(command => command.PlanId).GreaterThan(0);
    }
}
