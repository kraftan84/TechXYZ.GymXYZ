using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetSubscriptionOverviewQueryHandler
    : IRequestHandler<GetSubscriptionOverviewQuery, SubscriptionOverviewDto>
{
    /// <summary>"Encaissements récents" is the last week, as the card's own caption says.</summary>
    public const int RecentPaymentDays = 7;

    /// <summary>
    /// How far back a lapsed cover still belongs on the suivi table.
    /// <para>
    /// The screen is a worklist: covers running, covers about to run out, and
    /// covers that stopped recently enough that somebody should still be chasing
    /// them. A subscription that ended last spring is history — it belongs to
    /// the member's record, not to today's work — and now that every member
    /// carries the chain of what they bought, reading all of it would bury the
    /// dozen rows that matter under several hundred that do not.
    /// </para>
    /// </summary>
    public const int LapsedCoverDays = 90;

    private readonly IGymDbContext _dbContext;

    public GetSubscriptionOverviewQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionOverviewDto> Handle(
        GetSubscriptionOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var horizon = SubscriptionStatusRules.HorizonFrom(today);

        var plans = await _dbContext.Plans
            .AsNoTracking()
            .Where(plan => plan.IsActive)
            .OrderBy(plan => plan.Rank)
            .ThenBy(plan => plan.Id)
            .SelectPlanDto(today)
            .ToListAsync(cancellationToken);

        // Covers that have begun and have not been over for long — lapsed ones
        // included, because "En retard" is a row of this table and leaving
        // expired covers out would empty the very filter the screen exists to
        // work through.
        var rows = await _dbContext.Subscriptions
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
                // cover just projected. It cannot be computed in SQL without
                // saying the rule a second time.
                SubscriptionStatus.Active))
            .ToListAsync(cancellationToken);

        // One row per member, not one per subscription. A member who has renewed
        // four times has four covers inside the window, and the suivi asks a
        // question about people: where does this member stand today. Which of
        // their covers answers that is the same "healthiest one" decision the
        // members table makes, taken by the same rule.
        var resolved = rows
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
            .ToList();

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(payment =>
                payment.IsActive &&
                payment.Member!.IsActive &&
                payment.Date >= today.AddDays(-RecentPaymentDays))
            .OrderByDescending(payment => payment.Date)
            .ThenByDescending(payment => payment.Id)
            .Select(payment => new PaymentRowDto(
                payment.Id,
                payment.MemberId,
                payment.Member!.FirstName,
                payment.Member.LastName,
                payment.Date,
                payment.Label,
                payment.Amount,
                payment.Method,
                payment.Status))
            .ToListAsync(cancellationToken);

        var memberCount = await _dbContext.Members
            .CountAsync(member => member.IsActive, cancellationToken);

        return new SubscriptionOverviewDto(
            await BuildKpisAsync(resolved, memberCount, today, cancellationToken),
            plans,
            resolved,
            payments);
    }

    private async Task<SubscriptionKpisDto> BuildKpisAsync(
        IReadOnlyList<SubscriptionRowDto> rows,
        int memberCount,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var mrr = await MonthlyRecurringRevenueAsync(today, cancellationToken);

        // A month back, over the covers that were running then — the same sum,
        // asked of a different day. Not a stored history: the subscriptions
        // themselves are the record of who was paying what and when.
        var lastMonth = await MonthlyRecurringRevenueAsync(today.AddMonths(-1), cancellationToken);

        var running = rows
            .Where(row => row.Status is SubscriptionStatus.Active or SubscriptionStatus.ExpiringSoon)
            .ToList();

        var late = rows.Where(row => row.Status == SubscriptionStatus.Late).ToList();

        return new SubscriptionKpisDto(
            mrr,
            // No percentage against nothing: a gym with no recurring revenue last
            // month has not grown infinitely, it has just started.
            lastMonth > 0 ? (int)Math.Round((mrr - lastMonth) * 100m / lastMonth) : null,
            running.Count,
            memberCount,
            rows.Count(row => row.Status == SubscriptionStatus.ExpiringSoon),
            late.Count,
            late.Sum(row => row.Price),
            // "Panier moyen par abonné actif" — over everybody still covered,
            // packs included, because they are subscribers who paid something.
            running.Count == 0 ? 0m : decimal.Round(mrr / running.Count, 2));
    }

    /// <summary>
    /// The monthly-normalised sum of the recurring covers running on a given day.
    /// <para>
    /// Read off <c>Subscription.MonthlyPrice</c>, snapshot at the sale, rather
    /// than through the plan: a formule whose price is raised tomorrow must not
    /// restate what the members already on it have been contributing. Packs carry
    /// nought there by construction, which is the business rule made structural
    /// rather than repeated in a <c>where</c> clause.
    /// </para>
    /// </summary>
    private async Task<decimal> MonthlyRecurringRevenueAsync(DateOnly on, CancellationToken cancellationToken)
    {
        return await _dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.IsActive &&
                subscription.Member!.IsActive &&
                subscription.StartedOn <= on &&
                subscription.EndsOn >= on)
            .SumAsync(subscription => subscription.MonthlyPrice, cancellationToken);
    }
}
