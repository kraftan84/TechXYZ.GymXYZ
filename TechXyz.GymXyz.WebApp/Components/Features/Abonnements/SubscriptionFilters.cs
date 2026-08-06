using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Abonnements;

/// <summary>
/// The wordings, tones and tallies of the Abonnements screens. Static so the two
/// presentations — desktop and mobile — cannot end up saying different things
/// about the same cover.
/// <para>
/// The chips themselves come from <see cref="SubscriptionLabels"/>, shared with
/// the member record: a subscription chipped "En retard" here and something else
/// there would read as two different things.
/// </para>
/// </summary>
public static class SubscriptionFilters
{
    /// <summary>
    /// The four filter chips, in the prototype's order. Null is "Tous" — the
    /// chip that filters nothing.
    /// </summary>
    public static readonly (string Label, SubscriptionStatus? Status)[] Chips =
    [
        ("Tous", null),
        ("Actifs", SubscriptionStatus.Active),
        ("Expire bientôt", SubscriptionStatus.ExpiringSoon),
        ("En retard", SubscriptionStatus.Late)
    ];

    public static IReadOnlyList<SubscriptionRowDto> Apply(
        IReadOnlyList<SubscriptionRowDto> rows,
        SubscriptionStatus? status) =>
        status is null ? rows : [.. rows.Where(row => row.Status == status)];

    public static int CountOf(SubscriptionOverviewDto overview, SubscriptionStatus? status) =>
        status is { } value ? overview.CountOf(value) : overview.Subscriptions.Count;

    /// <summary>
    /// The échéance cell: what actually runs out first.
    /// <para>
    /// A pack still inside its dates counts entries, because that is what its
    /// holder will hit first — "3 séances restantes". Everything else counts
    /// days, and once the days are gone it says how long ago.
    /// </para>
    /// </summary>
    public static string Deadline(SubscriptionRowDto row, DateOnly today)
    {
        if (row.DaysLeft(today) is not { } days)
        {
            return GxFormats.Plural(row.Cover.EntriesLeft ?? 0, "séance restante", "séances restantes");
        }

        if (days < 0)
        {
            return $"Échue depuis {GxFormats.Plural(-days, "j", "j")}";
        }

        var date = GxFormats.DayAndMonth(row.Cover.EndsOn);

        // "J-18" only inside the month that matters; beyond it the date alone is
        // the useful half, and a "J-250" is noise.
        return days is > 0 and <= 30 ? $"{date} · J-{days}" : days == 0 ? "Aujourd'hui" : date;
    }

    /// <summary>The icon beside the échéance — a card for entries, an alert once lapsed.</summary>
    public static string DeadlineIcon(SubscriptionRowDto row, DateOnly today) =>
        row.DaysLeft(today) switch
        {
            null => GxIconPaths.Card,
            < 0 => GxIconPaths.Alert,
            _ => GxIconPaths.Calendar
        };

    /// <summary>
    /// The gauge on this table fills from the calendar, not from the entries —
    /// except on a pack, where the entries are what runs out. The members table
    /// fills its own bar differently on purpose; both are the prototype's.
    /// </summary>
    public static int Gauge(SubscriptionRowDto row, DateOnly today) =>
        row.Cover.Kind == PlanKind.CreditPack
            ? row.Cover.CreditsPercent
            : row.Cover.PeriodPercentRemaining(today);

    public static GxTone GaugeTone(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Late or SubscriptionStatus.Ended => GxTone.Danger,
        SubscriptionStatus.ExpiringSoon => GxTone.Warning,
        _ => GxTone.Brand
    };

    /// <summary>
    /// The action at the end of a row, which is different work depending on where
    /// the cover stands: money to collect, a cover to renew, or nothing pressing.
    /// </summary>
    public static RowAction ActionFor(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Late => RowAction.Collect,
        SubscriptionStatus.ExpiringSoon => RowAction.Renew,
        _ => RowAction.Manage
    };

    /// <summary>"112 abonnements actifs · 6 expirent cette semaine".</summary>
    public static string Subtitle(SubscriptionOverviewDto overview)
    {
        var active = GxFormats.Plural(overview.Kpis.ActiveCount, "abonnement actif", "abonnements actifs");

        return overview.Kpis.ExpiringCount == 0
            ? active
            : $"{active} · {GxFormats.Plural(overview.Kpis.ExpiringCount, "expire", "expirent")} cette semaine";
    }

    /// <summary>"sur 128 membres" under the actifs tile.</summary>
    public static string ActiveCaption(SubscriptionKpisDto kpis) =>
        $"sur {GxFormats.Plural(kpis.MemberCount, "membre", "membres")}";

    /// <summary>"+6 % vs le mois dernier", or nothing when there is no month to compare with.</summary>
    public static string? MrrDelta(SubscriptionKpisDto kpis) =>
        kpis.MrrDeltaPercent is { } delta
            ? $"{(delta >= 0 ? "+" : "")}{delta} % vs le mois dernier"
            : null;

    public enum RowAction
    {
        Collect,
        Renew,
        Manage
    }
}
