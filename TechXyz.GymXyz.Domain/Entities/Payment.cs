using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Money that changed hands, recorded after the fact. Nothing here takes a
/// payment — there is no provider in the product — so a row is the staff saying
/// "this arrived", or "this came back".
/// <para>
/// A rejected row is what separates a subscription that merely ended from one
/// that is late: the cover alone cannot tell them apart.
/// </para>
/// </summary>
public class Payment : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    /// <summary>
    /// What it paid for. Optional: a member can settle something that no longer
    /// has a subscription behind it, and losing the subscription must not take
    /// the accounting with it.
    /// </summary>
    public int? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>What the row reads — the plan name at the time of the payment.</summary>
    public string Label { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Collected;
}
