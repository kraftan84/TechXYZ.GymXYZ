using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One member's copy of a <see cref="Plan"/>: what they bought, when it runs out
/// and what is left of it.
/// <para>
/// The price and the credit count are copied off the plan at the sale rather
/// than read through it, for the reason <see cref="Session"/> copies its
/// capacity off the template: raising a price must not rewrite what the members
/// already on that plan are paying, and shortening a pack must not shrink the
/// gauge of the ones already sold.
/// </para>
/// <para>
/// There is no <c>Status</c> column, deliberately. The standing follows the
/// clock and the payments, so storing it would make it wrong every minute
/// between two sweeps — the same reason lot 6 refused to store whether a session
/// was under way. <c>SubscriptionStatusRules</c> derives it.
/// </para>
/// </summary>
public class Subscription : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public int PlanId { get; set; }
    public Plan? Plan { get; set; }

    public DateOnly StartedOn { get; set; }

    /// <summary>
    /// Last day covered. A pack has one as much as a monthly plan does — "10
    /// entrées valables 4 mois" runs out whether or not the entries were used.
    /// </summary>
    public DateOnly EndsOn { get; set; }

    /// <summary>
    /// Entries left on a pack, decremented by the attendance sheet — never by
    /// the sign-up, or a no-show would eat a credit without a session. Null on a
    /// recurring subscription, where access is not counted and the gauge reads
    /// "∞".
    /// </summary>
    public int? CreditsRemaining { get; set; }

    /// <summary>
    /// Entries the pack was sold with, snapshot from <see cref="Plan.CreditCount"/>.
    /// This is the denominator the "3/10" and its gauge read; without it, a plan
    /// edited afterwards would change the past.
    /// </summary>
    public int? CreditsTotal { get; set; }

    public bool AutoRenew { get; set; }

    /// <summary>
    /// What the member pays, as the table prints it — "49 € / mois". Snapshot at
    /// the sale for the same reason as <see cref="CreditsTotal"/>.
    /// </summary>
    public string PriceLabel { get; set; } = string.Empty;

    public ICollection<Payment>? Payments { get; set; }
}
