using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Everything the Abonnements screen draws, in one read: the four KPI, the
/// formules, the suivi and the recent encaissements.
/// <para>
/// One query rather than four because the page shows them together and they have
/// to agree — a MRR counted at one moment beside a list counted at another is two
/// answers to the same question.
/// </para>
/// </summary>
public sealed record SubscriptionOverviewDto(
    SubscriptionKpisDto Kpis,
    IReadOnlyList<PlanDto> Plans,
    IReadOnlyList<SubscriptionRowDto> Subscriptions,
    IReadOnlyList<PaymentRowDto> Payments)
{
    public static SubscriptionOverviewDto Empty { get; } =
        new(SubscriptionKpisDto.Empty, [], [], []);

    public int CountOf(SubscriptionStatus status) =>
        Subscriptions.Count(row => row.Status == status);

    /// <summary>
    /// Members covered by each plan, for the "Répartition" bars. Counted off the
    /// rows on screen so the bars and the table cannot disagree.
    /// </summary>
    public int MemberCountOf(int planId) =>
        Subscriptions.Count(row =>
            row.Cover.PlanId == planId && row.Status != SubscriptionStatus.Ended);
}

/// <summary>
/// The four tiles. The MRR is computed, never stored — the monthly-normalised
/// sum of the <b>recurring</b> covers running today. Credit packs are outside it
/// by an explicit business decision: a pack is bought once, and smoothing it into
/// a recurring revenue would have the figure claim money that will not come
/// again.
/// </summary>
public sealed record SubscriptionKpisDto(
    decimal Mrr,
    int? MrrDeltaPercent,
    int ActiveCount,
    int MemberCount,
    int ExpiringCount,
    int LateCount,
    decimal LateAmount,
    decimal AverageBasket)
{
    public static SubscriptionKpisDto Empty { get; } = new(0m, null, 0, 0, 0, 0, 0m, 0m);
}

/// <summary>One row of the suivi table: who, on what, where it stands.</summary>
public sealed record SubscriptionRowDto(
    int SubscriptionId,
    int MemberId,
    string FirstName,
    string LastName,
    SubscriptionCoverDto Cover,
    decimal Price,
    DateOnly? LastReminderSentOn,
    SubscriptionStatus Status)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Days to run, negative once the cover has lapsed. Null on a pack that has
    /// spent its entries but still has calendar left — there the number that
    /// means something is the entries, not the date.
    /// </summary>
    public int? DaysLeft(DateOnly today) =>
        Cover.Kind == PlanKind.CreditPack && Cover.EndsOn >= today
            ? null
            : Cover.EndsOn.DayNumber - today.DayNumber;
}

/// <summary>One row of "Encaissements récents".</summary>
public sealed record PaymentRowDto(
    int Id,
    int MemberId,
    string FirstName,
    string LastName,
    DateOnly Date,
    string Label,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
