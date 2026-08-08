namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// What the directory hands back when an address really does have an account
/// that may be reset: the one-shot token, and the name to greet in the e-mail.
/// <para>
/// Null is the answer for every other case — unknown address, another customer's
/// account, an access that has been revoked. The caller must treat all of them
/// exactly as it treats a success, because the screen does: telling the two
/// apart is account enumeration.
/// </para>
/// </summary>
public sealed record PasswordResetTicket(string Email, string Token, string? DisplayName);
