namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One message, ready to leave. Carries no credential and names no provider:
/// what it takes to actually deliver this is the sender's business.
/// </summary>
public sealed record EmailMessage(
    string ToAddress,
    string? ToName,
    string Subject,
    string TextBody)
{
    /// <summary>
    /// The name the message appears to come from — the gym's, not GymXYZ's. The
    /// address it is sent from belongs to the service: a provider will not
    /// dispatch on behalf of a domain it does not control, so a customer's own
    /// address in <c>From</c> would land the message in spam or be refused
    /// outright.
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// Where a reply goes — the gym's own address. This is what makes the
    /// service <c>From</c> honest rather than a dead end: a member answering a
    /// relance reaches the gym, not a mailbox nobody reads.
    /// </summary>
    public string? ReplyToAddress { get; init; }

    public string? ReplyToName { get; init; }
}

/// <summary>
/// What became of a message. A failure is described, never thrown: the caller
/// has already saved something by the time this comes back, and an exception
/// here would undo work the user asked for because a mail server was down.
/// </summary>
public sealed record EmailDeliveryResult(bool IsSent, string? Failure = null)
{
    public static EmailDeliveryResult Sent { get; } = new(true);

    public static EmailDeliveryResult Failed(string reason) => new(false, reason);
}
