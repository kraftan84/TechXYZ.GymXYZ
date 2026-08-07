using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Records money that already changed hands. Nothing is taken here — there is no
/// payment provider in the product, and wiring one is a lot of its own. This
/// writes down a card that was tapped at the desk, a cheque that arrived, a
/// direct debit that cleared.
/// <para>
/// Which is why <see cref="Status"/> is an input rather than always
/// <c>Collected</c>: a rejected direct debit is exactly the row somebody comes to
/// this screen to enter.
/// </para>
/// </summary>
public sealed class RecordPaymentCommand : IRequest<int>, IManagerOnly
{
    public RecordPaymentCommand(
        int memberId,
        int? subscriptionId,
        decimal amount,
        PaymentMethod method,
        DateOnly? date,
        PaymentStatus status = PaymentStatus.Collected)
    {
        MemberId = memberId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        Method = method;
        Date = date ?? DateOnly.FromDateTime(DateTime.Today);
        Status = status;
    }

    public int MemberId { get; }

    /// <summary>
    /// What it paid for. Optional: somebody can settle an old balance whose
    /// subscription is long gone, and refusing that would leave the money
    /// unrecorded.
    /// </summary>
    public int? SubscriptionId { get; }

    public decimal Amount { get; }
    public PaymentMethod Method { get; }
    public DateOnly Date { get; }
    public PaymentStatus Status { get; }
}
