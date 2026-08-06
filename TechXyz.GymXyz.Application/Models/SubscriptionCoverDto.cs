using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One subscription, reduced to everything needed to say how it stands and to
/// print its row — and nothing else.
/// <para>
/// The members table, the attendance sheets and the suivi table all read the
/// same cover, which is why the gauge and the "3/10" are computed here once
/// rather than three times in three screens.
/// </para>
/// </summary>
public sealed record SubscriptionCoverDto(
    int SubscriptionId,
    int PlanId,
    string PlanName,
    PlanKind Kind,
    DateOnly StartedOn,
    DateOnly EndsOn,
    int? CreditsRemaining,
    int? CreditsTotal,
    string PriceLabel,
    bool AutoRenew,
    decimal Price,
    decimal CollectedAmount,
    bool HasFailedPayment)
{
    /// <summary>
    /// Whether money is still owed on this cover — what separates a subscription
    /// that merely ended from one that is late.
    /// <para>
    /// Two conditions, and both are needed. A recorded failure alone is not
    /// enough: a direct debit that bounced and was settled in cash at the desk
    /// the same afternoon is paid, and a row that kept saying "En retard" after
    /// the gym took the money would send somebody to chase it twice. A shortfall
    /// alone is not enough either: most covers here have no payment rows at all,
    /// and treating silence as debt would put half a demo database in arrears.
    /// </para>
    /// </summary>
    public bool HasOutstandingPayment => HasFailedPayment && CollectedAmount < Price;

    /// <summary>
    /// What the credits column reads. A pack counts its entries; a recurring
    /// plan does not count access at all, and says so.
    /// </summary>
    public string CreditsLabel => Kind == PlanKind.CreditPack
        ? $"{Math.Max(0, CreditsRemaining ?? 0)}/{CreditsTotal ?? 0}"
        : "∞";

    /// <summary>
    /// The gauge beside the credits, 0–100. A pack shows the entries it has
    /// left; a recurring plan has nothing to run down and shows full, which is
    /// what "∞" beside a whole bar means on the members table.
    /// </summary>
    public int CreditsPercent
    {
        get
        {
            if (Kind != PlanKind.CreditPack)
            {
                return 100;
            }

            var total = CreditsTotal ?? 0;
            return total <= 0
                ? 0
                : Math.Clamp((int)Math.Round(Math.Max(0, CreditsRemaining ?? 0) * 100d / total), 0, 100);
        }
    }

    /// <summary>
    /// How much of the cover is left to run, 0–100 — the suivi table's gauge and
    /// the record's ring.
    /// <para>
    /// Deliberately not the same bar as <see cref="CreditsPercent"/>: the
    /// prototype fills one from the entries and the other from the calendar, and
    /// on a monthly plan they say different things. Laetitia Moriceau is at 100
    /// on the members table (nothing counts down) and at 62 on the abonnements
    /// table (eighteen days of thirty left).
    /// </para>
    /// </summary>
    public int PeriodPercentRemaining(DateOnly today)
    {
        var span = EndsOn.DayNumber - StartedOn.DayNumber;
        if (span <= 0)
        {
            return 0;
        }

        var left = EndsOn.DayNumber - today.DayNumber;
        return Math.Clamp((int)Math.Round(left * 100d / span), 0, 100);
    }

    /// <summary>Entries left, or null when the plan does not count them.</summary>
    public int? EntriesLeft => Kind == PlanKind.CreditPack ? Math.Max(0, CreditsRemaining ?? 0) : null;
}
