using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSiteCommandValidator : AbstractValidator<CreateSiteCommand>
{
    public CreateSiteCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Street).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(command => command.City).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Country).NotEmpty().MaximumLength(100);
    }
}
