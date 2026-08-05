using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.SiteId).GreaterThan(0);
    }
}
