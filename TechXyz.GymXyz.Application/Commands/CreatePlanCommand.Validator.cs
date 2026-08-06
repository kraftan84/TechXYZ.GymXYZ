using FluentValidation;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(PlanRules.NameRequired)
            .MaximumLength(80);

        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage(PlanRules.PriceRequired);

        RuleFor(command => command.ValidityMonths)
            .InclusiveBetween(1, 60).WithMessage(PlanRules.ValidityOutOfRange);

        // A pack with no entries is a plan that cannot be used, and the gauge
        // would divide by nought the first time somebody bought it.
        RuleFor(command => command.CreditCount)
            .NotNull().GreaterThan(0)
            .When(command => command.Kind == PlanKind.CreditPack)
            .WithMessage(PlanRules.CreditCountRequired);
    }
}
