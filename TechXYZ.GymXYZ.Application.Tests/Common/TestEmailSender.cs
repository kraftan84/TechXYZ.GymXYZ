using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Collects what would have gone out. The provider itself is not exercised here
/// — what a handler owes is the right message to the right person, and whether
/// Brevo accepted it is a question for the sender's own tests.
/// </summary>
internal sealed class TestEmailSender : IEmailSender
{
    private readonly bool _fails;

    public TestEmailSender(bool fails = false) => _fails = fails;

    public List<EmailMessage> Sent { get; } = [];

    public EmailMessage Single => Sent.Count == 1
        ? Sent[0]
        : throw new InvalidOperationException($"Expected one message, found {Sent.Count}.");

    public Task<EmailDeliveryResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        Sent.Add(message);

        return Task.FromResult(_fails
            ? EmailDeliveryResult.Failed("test")
            : EmailDeliveryResult.Sent);
    }
}
