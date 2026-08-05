using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.SiteId).GreaterThan(0);
    }
}
