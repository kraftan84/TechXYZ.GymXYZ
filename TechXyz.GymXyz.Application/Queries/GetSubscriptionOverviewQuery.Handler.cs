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

        // One row per member, not one per subscription — and read through the
        // shared reader so the Accueil's « N abonnements expirent » counts the
        // very rows this table displays.
        var resolved = await SubscriptionStanding.LoadAsync(
            _dbContext, today, horizon, cancellationToken);

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
