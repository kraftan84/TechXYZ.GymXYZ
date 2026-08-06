using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Chases a member whose cover is unsettled.
/// <para>
/// <b>Nothing leaves the building yet.</b> Messaging — the channel, the
/// templates, the per-notification switches — arrives with the Réglages at lot 8,
/// and lot 6 already drew « Relancer » disabled on the absentees card for exactly
/// this reason. The command lands now, records the intent and stamps
/// <c>Subscription.LastReminderSentOn</c>; lot 8 gives it something to send down
/// and the control stops being disabled. Until then the screen says so in a
/// tooltip rather than offering a button that does nothing.
/// </para>
/// <para>
/// The stamp is not bookkeeping for its own sake: it is what stops four chases
/// going out in one morning once there is a channel, and what lets the row say
/// when the last one went.
/// </para>
/// </summary>
public sealed class SendPaymentReminderCommand : IRequest<bool>
{
    public SendPaymentReminderCommand(int subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public int SubscriptionId { get; }
}
