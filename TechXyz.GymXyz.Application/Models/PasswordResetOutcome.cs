namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// What became of a reset attempt, in the two shapes the screen can show: a link
/// that no longer works, or a password the rules refused.
/// </summary>
/// <param name="Succeeded">The password was changed and other devices signed out.</param>
/// <param name="LinkNoLongerValid">
/// Expired, already used, or pointing at an account that cannot be reset. All
/// three say the same thing on screen — which of them it was is never the user's
/// business, and separating them would leak whether the address exists.
/// </param>
/// <param name="PasswordErrors">
/// Identity's own refusals, already worded for a reader. Empty unless the
/// password itself was the problem.
/// </param>
public sealed record PasswordResetOutcome(
    bool Succeeded,
    bool LinkNoLongerValid,
    IReadOnlyList<string> PasswordErrors)
{
    public static PasswordResetOutcome Ok() => new(true, false, []);

    public static PasswordResetOutcome DeadLink() => new(false, true, []);

    public static PasswordResetOutcome Refused(IReadOnlyList<string> errors) => new(false, false, errors);
}
