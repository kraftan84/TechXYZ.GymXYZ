using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
    public DeletePlanCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
