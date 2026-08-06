using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Where every member stands today — one row per member, never one per
/// subscription.
/// <para>
/// That distinction is the whole reason this lives here rather than inside the
/// Abonnements handler. A member who has renewed four times has four covers
/// inside the window, and « 4 abonnements expirent » is a statement about
/// people. Counting the entity instead would give a different, larger number
/// than the Abonnements screen shows for the same gym on the same day — two
/// answers to one question, which is exactly what the dashboard was told not to
/// introduce.
/// </para>
/// <para>
/// <see cref="SubscriptionStatusRules"/> still owns what a status <i>is</i>.
/// This owns which cover gets to speak for a member.
/// </para>
/// </summary>
public static class SubscriptionStanding
{
    /// <summary>
    /// How far back a lapsed cover still belongs on a worklist.
    /// <para>
    /// The screens that read this are worklists: covers running, covers about to
    /// run out, and covers that stopped recently enough that somebody should
    /// still be chasing them. A subscription that ended last spring is history —
    /// it belongs to the member's record — and now that every member carries the
    /// chain of what they bought, reading all of it would bury the dozen rows
    /// that matter under several hundred that do not.
    /// </para>
    /// </summary>
    public const int LapsedCoverDays = 90;

    /// <summary>
    /// The governing cover of every member with one, resolved and sorted by due
    /// date. One query; the resolution happens in memory because it cannot be
    /// written in SQL without saying <see cref="SubscriptionStatusRules"/> a
    /// second time.
    /// </summary>
    public static async Task<IReadOnlyList<SubscriptionRowDto>> LoadAsync(
        IGymDbContext dbContext,
        DateOnly today,
        DateOnly horizon,
        CancellationToken cancellationToken)
    {
        // Covers that have begun and have not been over for long — lapsed ones
        // included, because "En retard" is one of the answers and leaving expired
        // covers out would empty the very count the alert exists to raise.
        var rows = await dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.IsActive &&
                subscription.Member!.IsActive &&
                subscription.StartedOn <= today &&
                subscription.EndsOn >= today.AddDays(-LapsedCoverDays))
            .OrderBy(subscription => subscription.EndsOn)
            .ThenBy(subscription => subscription.Member!.LastName)
            .Select(subscription => new SubscriptionRowDto(
                subscription.Id,
                subscription.MemberId,
                subscription.Member!.FirstName,
                subscription.Member.LastName,
                new SubscriptionCoverDto(
                    subscription.Id,
                    subscription.PlanId,
                    subscription.Plan!.Name,
                    subscription.Plan.Kind,
                    subscription.StartedOn,
                    subscription.EndsOn,
                    subscription.CreditsRemaining,
                    subscription.CreditsTotal,
                    subscription.PriceLabel,
                    subscription.AutoRenew,
                    subscription.Price,
                    subscription.Payments!
                        .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                        .Sum(payment => (decimal?)payment.Amount) ?? 0m,
                    subscription.Payments!.Any(payment =>
                        payment.IsActive && payment.Status != PaymentStatus.Collected)),
                subscription.Price,
                subscription.LastReminderSentOn,
                // Placeholder: the status is derived below, in memory, from the
                // cover just projected.
                SubscriptionStatus.Active))
            .ToListAsync(cancellationToken);

        return Resolve(rows, today, horizon);
    }

    /// <summary>
    /// Statuses onto the projected covers, then one row per member — the
    /// healthiest cover speaking for them, by the same
    /// <see cref="SubscriptionStatusRules.Governing"/> rule the members table
    /// uses. A pack bought on top of a monthly plan must not have the member
    /// reading « En retard » the day the pack runs dry.
    /// </summary>
    public static IReadOnlyList<SubscriptionRowDto> Resolve(
        IEnumerable<SubscriptionRowDto> rows,
        DateOnly today,
        DateOnly horizon) =>
    [
        .. rows
            .Select(row => row with
            {
                Status = SubscriptionStatusRules.Resolve(row.Cover, today, horizon)
            })
            .GroupBy(row => row.MemberId)
            .Select(perMember =>
            {
                var governing = SubscriptionStatusRules.Governing(
                    perMember.Select(row => row.Cover), today, horizon);

                return governing is null
                    ? perMember.First()
                    : perMember.First(row => row.Cover.SubscriptionId == governing.SubscriptionId);
            })
            .OrderBy(row => row.Cover.EndsOn)
            .ThenBy(row => row.LastName)
    ];
}
