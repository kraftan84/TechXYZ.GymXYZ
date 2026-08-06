using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RevokeAccessCommandHandler : IRequestHandler<RevokeAccessCommand, bool>
{
    private readonly IUserDirectory _userDirectory;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RevokeAccessCommand> _validator;

    public RevokeAccessCommandHandler(
        IUserDirectory userDirectory,
        ICurrentUserService currentUser,
        IValidator<RevokeAccessCommand> validator)
    {
        _userDirectory = userDirectory;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<bool> Handle(RevokeAccessCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var accounts = await _userDirectory.GetTenantUsersAsync(cancellationToken);
        var target = accounts.FirstOrDefault(account => account.UserId == request.UserId);

        if (target is null)
        {
            return false;
        }

        // Locking yourself out is a one-way door: the screen that undoes it is
        // the one you have just lost.
        if (string.Equals(target.Email, _currentUser.UserName, StringComparison.OrdinalIgnoreCase))
        {
            throw ValidationFailures.Refuse(
                SettingsFieldNames.Account, SettingsRules.CannotRevokeSelf);
        }

        if (target.Role == GymRoleNames.GymManager)
        {
            var otherManagers = accounts.Count(account =>
                account.UserId != target.UserId &&
                account.Role == GymRoleNames.GymManager &&
                !account.IsRevoked);

            if (otherManagers == 0)
            {
                throw ValidationFailures.Refuse(
                    SettingsFieldNames.Account, SettingsRules.LastManagerStands);
            }
        }

        return await _userDirectory.RevokeAsync(request.UserId, cancellationToken);
    }
}
