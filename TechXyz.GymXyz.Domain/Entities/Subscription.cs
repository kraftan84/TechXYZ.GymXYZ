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

    /// <summary>
    /// What one period of this cover costs — 49 for a monthly plan, 490 for a
    /// yearly one, 120 for a pack. This is the figure "180 € à encaisser" is a
    /// sum of.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The same money normalised to the month, and nought for a pack — a single
    /// purchase is not a monthly revenue.
    /// <para>
    /// Stored rather than divided at read time because the MRR must not move
    /// when a price does: <c>UpdatePlanCommand</c> can raise a formule tomorrow,
    /// and reading the revenue through the plan would restate what every
    /// existing subscriber has been contributing since the day they signed.
    /// That is the same reason <see cref="PriceLabel"/> and
    /// <see cref="CreditsTotal"/> are snapshots, applied to the one figure the
    /// business actually steers on.
    /// </para>
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// When somebody last chased this cover for payment. Null means never.
    /// <para>
    /// Written by <c>SendPaymentReminderCommand</c>, which has no channel to
    /// send down until messaging arrives at lot 8 — so for now it records the
    /// intent and nothing leaves the building. Keeping the date means the
    /// screen can say when the last chase went out instead of letting somebody
    /// send four in a morning.
    /// </para>
    /// </summary>
    public DateOnly? LastReminderSentOn { get; set; }

    public ICollection<Payment>? Payments { get; set; }
}
