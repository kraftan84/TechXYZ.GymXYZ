using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Spends a reset link and sets the new password. Public, like the request that
/// produced it — the link <em>is</em> the authentication.
/// </summary>
public sealed class ResetPasswordCommand : IRequest<PasswordResetOutcome>
{
    public ResetPasswordCommand(string email, string token, string newPassword)
    {
        Email = email.Trim();
        Token = token;
        NewPassword = newPassword;
    }

    public string Email { get; }

    public string Token { get; }

    public string NewPassword { get; }
}
