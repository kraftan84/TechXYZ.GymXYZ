using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Chases a member whose cover is unsettled, and now actually sends.
/// <para>
/// The stamp on <c>Subscription.LastReminderSentOn</c> is what stops four chases
/// going out in one morning, and what lets the row say when the last one went.
/// It is written before the message leaves and kept whatever the send does: a
/// relance that failed to reach a mail server was still a relance the gym
/// decided to make, and losing the record would have somebody send a second one
/// minutes later.
/// </para>
/// <para>
/// The gym's « Relance avant échéance » switch is consulted first. Manual though
/// this is, a customer that has turned member relances off has said something,
/// and the answer comes back as <see cref="NotificationOutcomeDto.Suppressed"/>
/// so the screen can say why rather than appear to have done nothing.
/// </para>
/// </summary>
public sealed class SendPaymentReminderCommand : IRequest<NotificationOutcomeDto>
{
    public SendPaymentReminderCommand(int subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public int SubscriptionId { get; }
}
