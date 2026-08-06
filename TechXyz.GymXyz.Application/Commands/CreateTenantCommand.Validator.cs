using System.Text.RegularExpressions;
using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed partial class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(TenantRules.NameRequired)
            .MaximumLength(120);

        RuleFor(command => command.Slug)
            .NotEmpty().WithMessage(TenantRules.SlugRequired)
            .MaximumLength(63)
            .Must(slug => HostLabel().IsMatch(slug.Trim().ToLowerInvariant()))
            .WithMessage(TenantRules.SlugInvalid)
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.ThemeKey)
            .NotEmpty().WithMessage(TenantRules.ThemeRequired);
    }

    /// <summary>
    /// The slug becomes a host prefix — <c>teamtrainers</c>.gymxyz.fr — so it is
    /// held to what a DNS label allows: lowercase, digits and inner hyphens.
    /// </summary>
    [GeneratedRegex("^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")]
    private static partial Regex HostLabel();
}
