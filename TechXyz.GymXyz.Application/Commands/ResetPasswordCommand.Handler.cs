using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, PasswordResetOutcome>
{
    private readonly IUserDirectory _userDirectory;

    public ResetPasswordCommandHandler(IUserDirectory userDirectory)
    {
        _userDirectory = userDirectory;
    }

    public Task<PasswordResetOutcome> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Every rule that matters here is Identity's own — the token's age, the
        // fact it is single-use, the password's shape — so this passes straight
        // through rather than re-stating any of them a second time and worse.
        return _userDirectory.CompletePasswordResetAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);
    }
}
