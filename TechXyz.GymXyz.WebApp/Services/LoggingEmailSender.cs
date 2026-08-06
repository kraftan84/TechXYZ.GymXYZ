using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Writes the message to the log instead of sending it. What runs whenever no
/// API key is configured.
/// <para>
/// This is not a stub left behind: it is what stops a developer's laptop
/// e-mailing three hundred real members the first time somebody cancels a
/// session against a copy of production data. Sending is opt-in, by configuring
/// a key.
/// </para>
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<EmailDeliveryResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "E-mail not sent (no provider configured). To: {Recipient} · Subject: {Subject}{NewLine}{Body}",
            message.ToAddress, message.Subject, Environment.NewLine, message.TextBody);

        // Reported as sent: from the caller's point of view the message left as
        // far as this deployment is concerned, and a screen that cried failure
        // on every development cancellation would train people past the warning.
        return Task.FromResult(EmailDeliveryResult.Sent);
    }
}
