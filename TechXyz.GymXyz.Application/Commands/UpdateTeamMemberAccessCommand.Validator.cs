using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTeamMemberAccessCommandValidator
    : AbstractValidator<UpdateTeamMemberAccessCommand>
{
    public UpdateTeamMemberAccessCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.RoleName)
            .Must(TeamAccessScopes.IsAssignable).WithMessage(SettingsRules.RoleNotAssignable);
    }
}
