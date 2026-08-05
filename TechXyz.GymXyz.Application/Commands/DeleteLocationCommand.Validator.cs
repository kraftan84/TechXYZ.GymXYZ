using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLocationCommandValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
