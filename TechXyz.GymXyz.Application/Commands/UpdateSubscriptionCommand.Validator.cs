using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.NumberOfLessons).GreaterThan(0);
        RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(command => command.StartDate);
    }
}
