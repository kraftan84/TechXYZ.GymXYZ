using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTenantBrandingCommandValidator
    : AbstractValidator<UpdateTenantBrandingCommand>
{
    public UpdateTenantBrandingCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty().WithMessage(TenantRules.NameRequired)
            .MaximumLength(120);

        RuleFor(command => command.ThemeKey)
            .NotEmpty().WithMessage(TenantRules.ThemeRequired);

        RuleFor(command => command.Baseline).MaximumLength(160);
        RuleFor(command => command.WordmarkText).MaximumLength(60);
        RuleFor(command => command.WordmarkPrefix).MaximumLength(30);
        RuleFor(command => command.WordmarkAccent).MaximumLength(30);
    }
}
