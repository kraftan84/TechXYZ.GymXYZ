using System.Linq.Expressions;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The one definition of how a subscription stands, for every screen that shows
/// a standing.
/// <para>
/// Nothing here is stored. A status that follows the clock and is written down
/// is wrong every minute between two sweeps — the reason lot 6 refused to keep a
/// "session under way" column, and the reason <c>Subscription</c> has no
/// <c>Status</c> even though the hand-off's data model lists one.
/// </para>
/// <para>
/// Two things end a subscription and they are not the same: running out of
/// calendar, and running out of entries. Two things separate an ended one from a
/// late one, and only one of them is a date — money outstanding is what makes
/// "En retard" mean something the cover alone cannot say.
/// </para>
/// <para>
/// <see cref="Resolve"/> answers over the projected cover, <see cref="Matches"/>
/// says the same thing over the entity so the database can filter and count
/// without loading rows. The two are written separately because a predicate over
/// a projected record does not translate to SQL; <c>SubscriptionStatusRulesTests</c>
/// pins them to each other through the real query, so they cannot drift.
/// </para>
/// </summary>
public static class SubscriptionStatusRules
{
    /// <summary>A cover ending within this many days reads "Expire bientôt".</summary>
    public const int ExpiringSoonWithinDays = 7;

    /// <summary>
    /// A pack down to this many entries reads "Expire bientôt" too, however long
    /// it still has to run. Three is what the prototype shows: Camille Durand,
    /// "3 séances restantes", warned.
    /// </summary>
    public const int ExpiringSoonAtOrBelowCredits = 3;

    public static DateOnly HorizonFrom(DateOnly today) => today.AddDays(ExpiringSoonWithinDays);

    public static SubscriptionStatus Resolve(SubscriptionCoverDto cover, DateOnly today, DateOnly horizon)
    {
        var spent = cover.Kind == PlanKind.CreditPack && (cover.CreditsRemaining ?? 0) <= 0;

        if (cover.EndsOn < today || spent)
        {
            return cover.HasOutstandingPayment ? SubscriptionStatus.Late : SubscriptionStatus.Ended;
        }

        if (cover.EndsOn <= horizon)
        {
            return SubscriptionStatus.ExpiringSoon;
        }

        return cover.Kind == PlanKind.CreditPack && cover.CreditsRemaining <= ExpiringSoonAtOrBelowCredits
            ? SubscriptionStatus.ExpiringSoon
            : SubscriptionStatus.Active;
    }

    /// <summary>
    /// The same four conditions, over the entity.
    /// <para>
    /// A subscription that has not started yet is none of these: it is a renewal
    /// booked ahead, and counting it would have a member reading "Actif" on a
    /// cover that begins next month.
    /// </para>
    /// </summary>
    public static Expression<Func<Subscription, bool>> Matches(
        SubscriptionStatus status,
        DateOnly today,
        DateOnly horizon)
    {
        return status switch
        {
            SubscriptionStatus.Active => subscription =>
                subscription.IsActive &&
                subscription.StartedOn <= today &&
                subscription.EndsOn > horizon &&
                (subscription.CreditsRemaining == null ||
                 subscription.CreditsRemaining > ExpiringSoonAtOrBelowCredits),

            SubscriptionStatus.ExpiringSoon => subscription =>
                subscription.IsActive &&
                subscription.StartedOn <= today &&
                subscription.EndsOn >= today &&
                (subscription.CreditsRemaining == null || subscription.CreditsRemaining > 0) &&
                (subscription.EndsOn <= horizon ||
                 (subscription.CreditsRemaining != null &&
                  subscription.CreditsRemaining <= ExpiringSoonAtOrBelowCredits)),

            // Owing is two conditions, exactly as SubscriptionCoverDto states
            // them: a payment recorded as failed, and a collected total that
            // still falls short. Either alone would be wrong — a bounced debit
            // settled in cash is paid, and a cover with no payment rows at all
            // is not in arrears just because nobody recorded anything.
            SubscriptionStatus.Late => subscription =>
                subscription.IsActive &&
                subscription.StartedOn <= today &&
                (subscription.EndsOn < today || subscription.CreditsRemaining <= 0) &&
                subscription.Payments!.Any(payment =>
                    payment.IsActive && payment.Status != PaymentStatus.Collected) &&
                subscription.Payments!
                    .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                    .Sum(payment => (decimal?)payment.Amount).GetValueOrDefault() < subscription.Price,

            _ => subscription =>
                subscription.IsActive &&
                subscription.StartedOn <= today &&
                (subscription.EndsOn < today || subscription.CreditsRemaining <= 0) &&
                !(subscription.Payments!.Any(payment =>
                      payment.IsActive && payment.Status != PaymentStatus.Collected) &&
                  subscription.Payments!
                      .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                      .Sum(payment => (decimal?)payment.Amount).GetValueOrDefault() < subscription.Price)
        };
    }

    /// <summary>
    /// The cover that governs a member with several — the healthiest one, in the
    /// declaration order of <see cref="SubscriptionStatus"/>. A pack bought on
    /// top of a monthly plan must not have the member reading "En retard" the
    /// day the pack runs dry; and between two finished ones, the late one is
    /// what somebody has to act on.
    /// </summary>
    public static SubscriptionCoverDto? Governing(
        IEnumerable<SubscriptionCoverDto> covers,
        DateOnly today,
        DateOnly horizon)
    {
        return covers
            .Where(cover => cover.StartedOn <= today)
            .OrderBy(cover => (int)Resolve(cover, today, horizon))
            .ThenByDescending(cover => cover.EndsOn)
            .FirstOrDefault();
    }
}
