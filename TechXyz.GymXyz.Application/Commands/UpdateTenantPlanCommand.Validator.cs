using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTenantPlanCommandValidator : AbstractValidator<UpdateTenantPlanCommand>
{
    public UpdateTenantPlanCommandValidator()
    {
        RuleFor(command => command.GymPlan).MaximumLength(80);
        RuleFor(command => command.PlanDescription).MaximumLength(160);

        RuleFor(command => command.PlanPrice)
            .InclusiveBetween(0m, 10_000m).WithMessage(TenantRules.PlanPriceOutOfRange)
            .When(command => command.PlanPrice is not null);

        // Null is unlimited and perfectly valid; zero is not, because a plan that
        // covers nobody is a plan nobody can be on.
        RuleFor(command => command.PlanMemberCap)
            .InclusiveBetween(1, 100_000).WithMessage(TenantRules.MemberCapOutOfRange)
            .When(command => command.PlanMemberCap is not null);
    }
}
