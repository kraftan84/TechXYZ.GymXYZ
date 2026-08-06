using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Turns a plan into one member's copy of it. Selling and renewing both come
/// through here so a renewal cannot quietly differ from the original sale.
/// </summary>
public static class SubscriptionFactory
{
    /// <summary>
    /// The cover a plan produces when bought on <paramref name="startedOn"/>.
    /// <para>
    /// The price and the entry count are copied rather than looked up later:
    /// raising a price must not change what the members already on it pay, and
    /// shortening a pack must not shrink a gauge already sold.
    /// </para>
    /// </summary>
    public static Subscription Create(Plan plan, int memberId, DateOnly startedOn, bool autoRenew)
    {
        return new Subscription
        {
            MemberId = memberId,
            PlanId = plan.Id,
            StartedOn = startedOn,
            EndsOn = EndOfCover(plan, startedOn),
            CreditsRemaining = plan.IsCredited ? plan.CreditCount : null,
            CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
            AutoRenew = autoRenew,
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = MonthlyPriceOf(plan)
        };
    }

    /// <summary>
    /// What one month of a plan is worth to the recurring revenue. A pack is
    /// worth nothing to it: it is bought once, and smoothing it in would have
    /// the MRR claim money that will not come again. The business asked for that
    /// rule explicitly, and if a blended figure is ever wanted it is a second
    /// indicator with its own name, not this one.
    /// </summary>
    public static decimal MonthlyPriceOf(Plan plan) =>
        plan.Kind == PlanKind.Recurring && plan.ValidityMonths > 0
            ? decimal.Round(plan.Price / plan.ValidityMonths, 2)
            : 0m;

    /// <summary>
    /// Last day a cover bought on <paramref name="startedOn"/> runs. The day
    /// before the anniversary, so a monthly plan started on the 8th ends on the
    /// 7th and the next one starts on the 8th — no day covered twice, none
    /// missed.
    /// </summary>
    public static DateOnly EndOfCover(Plan plan, DateOnly startedOn) =>
        startedOn.AddMonths(Math.Max(1, plan.ValidityMonths)).AddDays(-1);
}
