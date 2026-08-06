using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTeamMemberAccessCommandHandler
    : IRequestHandler<UpdateTeamMemberAccessCommand, bool>
{
    private readonly IUserDirectory _userDirectory;
    private readonly IValidator<UpdateTeamMemberAccessCommand> _validator;

    public UpdateTeamMemberAccessCommandHandler(
        IUserDirectory userDirectory,
        IValidator<UpdateTeamMemberAccessCommand> validator)
    {
        _userDirectory = userDirectory;
        _validator = validator;
    }

    public async Task<bool> Handle(
        UpdateTeamMemberAccessCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var accounts = await _userDirectory.GetTenantUsersAsync(cancellationToken);
        var target = accounts.FirstOrDefault(account => account.UserId == request.UserId);

        if (target is null)
        {
            return false;
        }

        // Demoting the last manager would leave the gym with nobody able to hand
        // the role back — including to themselves.
        if (target.Role == GymRoleNames.GymManager && request.RoleName != GymRoleNames.GymManager)
        {
            var otherManagers = accounts.Count(account =>
                account.UserId != target.UserId &&
                account.Role == GymRoleNames.GymManager &&
                !account.IsRevoked);

            if (otherManagers == 0)
            {
                throw ValidationFailures.Refuse(
                    SettingsFieldNames.Role, SettingsRules.LastManagerStands);
            }
        }

        return await _userDirectory.SetRoleAsync(request.UserId, request.RoleName, cancellationToken);
    }
}
