using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    public InviteTeamMemberCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage(SettingsRules.InvitationEmailRequired)
            .EmailAddress().WithMessage(SettingsRules.EmailInvalid)
            .MaximumLength(255);

        RuleFor(command => command.RoleName)
            .Must(TeamAccessScopes.IsAssignable).WithMessage(SettingsRules.RoleNotAssignable);
    }
}
