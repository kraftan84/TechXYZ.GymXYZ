using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(PlanRules.NameRequired)
            .MaximumLength(80);

        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage(PlanRules.PriceRequired);

        RuleFor(command => command.ValidityMonths)
            .InclusiveBetween(1, 60).WithMessage(PlanRules.ValidityOutOfRange);
    }
}
