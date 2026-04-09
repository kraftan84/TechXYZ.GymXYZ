using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(command => command.MemberId).GreaterThan(0);
        RuleFor(command => command.NumberOfLessons).GreaterThan(0);
        RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(command => command.StartDate);
    }
}
