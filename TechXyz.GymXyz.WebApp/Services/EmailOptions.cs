namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// What it takes to send. Configured under <c>Email</c>; the API key belongs in
/// user-secrets or the environment, never in a checked-in file.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// The address messages are dispatched from. A service address on a domain
    /// TechXYZ controls, because a provider will not send on behalf of a domain
    /// it cannot verify — a customer's own address here would be refused or
    /// filed as spam. The gym's name still leads, and replies go to the gym.
    /// </summary>
    public string FromAddress { get; set; } = "notifications@gymxyz.fr";

    /// <summary>Used only when a message names no gym — it always does in practice.</summary>
    public string FromName { get; set; } = "GymXYZ";

    /// <summary>
    /// Brevo's transactional key. Empty means nothing is sent: the logging
    /// implementation takes over, so development and tests need no account and
    /// no member ever receives a message from somebody's laptop.
    /// </summary>
    public string? ApiKey { get; set; }

    public string ApiUrl { get; set; } = "https://api.brevo.com/v3/smtp/email";
}
