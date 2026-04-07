using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCoachCommandValidator : AbstractValidator<DeleteCoachCommand>
{
    public DeleteCoachCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);
    }
}
