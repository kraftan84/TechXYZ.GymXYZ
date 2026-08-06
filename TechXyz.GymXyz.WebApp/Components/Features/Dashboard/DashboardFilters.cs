using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Features.Planning;
using TechXyz.GymXyz.WebApp.Components.Layout;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Dashboard;

/// <summary>
/// One alert of « À surveiller », already worded. The two presentations draw the
/// same three, so what they say is decided once.
/// </summary>
/// <param name="Href">Where the action goes — every alert leads to the screen that works it.</param>
public sealed record DashboardAlert(
    string Icon,
    GxTone Tone,
    string Title,
    string Detail,
    string Action,
    string Href);

/// <summary>
/// How the Accueil reads on screen. Static and stateless, so the desktop and
/// mobile presentations cannot word the same figure differently.
/// </summary>
public static class DashboardFilters
{
    /// <summary>"Semaine du 3 août au 9 août 2026" — the planning's own wording.</summary>
    public static string RangeLabel(DateOnly weekStart) => PlanningFilters.RangeLabel(weekStart);

    /// <summary>"Lun", over the number in a strip cell.</summary>
    public static string DayName(DateOnly day) => PlanningFilters.DayLabels[((int)day.DayOfWeek + 6) % 7];

    /// <summary>"28 cours · 6 coachs", or the count alone for a solo coach.</summary>
    public static string WeekMeta(DashboardDto dashboard, bool isSolo)
    {
        var classes = GxFormats.Plural(dashboard.WeekSessionCount, "cours", "cours");

        return isSolo || dashboard.WeekCoachCount == 0
            ? classes
            : $"{classes} · {GxFormats.Plural(dashboard.WeekCoachCount, "coach", "coachs")}";
    }

    /// <summary>"Aujourd'hui · 3 cours".</summary>
    public static string TodayTitle(DashboardDto dashboard) =>
        $"Aujourd'hui · {GxFormats.Plural(dashboard.TodayClasses.Count, "cours", "cours")}";

    /// <summary>
    /// The three alerts, in the prototype's order, keeping only those with
    /// something to report. An alert row reading "0 abonnements expirent" would
    /// be work where there is none.
    /// </summary>
    public static IReadOnlyList<DashboardAlert> Alerts(DashboardAlertsDto alerts)
    {
        var list = new List<DashboardAlert>();

        if (alerts.ExpiringCount > 0)
        {
            list.Add(new DashboardAlert(
                GxIconPaths.Card,
                GxTone.Warning,
                GxFormats.Plural(alerts.ExpiringCount, "abonnement expire", "abonnements expirent"),
                "Cette semaine — pensez à relancer",
                "Relancer",
                GxNavigation.Abonnements.Href));
        }

        if (alerts.LateCount > 0)
        {
            list.Add(new DashboardAlert(
                GxIconPaths.Alert,
                GxTone.Danger,
                GxFormats.Plural(alerts.LateCount, "paiement en retard", "paiements en retard"),
                $"{GxFormats.Amount(alerts.LateAmount)} à encaisser",
                "Voir",
                GxNavigation.Abonnements.Href));
        }

        if (alerts.SheetsToPoint > 0)
        {
            // The prototype titles this « Présences d'hier ». The rule reaches a
            // week back — a sheet forgotten on Friday is still here on Monday —
            // so the title says what is counted rather than when it happened.
            list.Add(new DashboardAlert(
                GxIconPaths.Check,
                GxTone.Brand,
                "Présences à pointer",
                GxFormats.Plural(alerts.SheetsToPoint, "cours à pointer", "cours à pointer"),
                "Pointer",
                GxNavigation.Presences.Href));
        }

        return list;
    }

    /// <summary>
    /// The tinted square behind an alert's icon. Inline rather than a class
    /// because the tones are the design system's own tokens and the prototype
    /// tints this square the same way — there is no new CSS in this lot.
    /// </summary>
    public static string AlertIconStyle(GxTone tone) => tone switch
    {
        GxTone.Warning => "background:var(--warning-50);color:var(--warning-600)",
        GxTone.Danger => "background:var(--danger-50);color:var(--color-danger)",
        _ => "background:var(--azure-50);color:var(--color-primary)"
    };

    /// <summary>The mark on a day that is a public holiday or falls in school holidays.</summary>
    public static string? DayMarkClass(SchoolCalendarDto calendar, DateOnly day) =>
        calendar.MarkFor(day) is { } mark ? CalendarFormats.MarkClass(mark.Kind) : null;

    /// <summary>
    /// Why « Diffuser le planning » and « Aperçu » are there but cannot be
    /// pressed. Said out loud on the button rather than left to be discovered.
    /// </summary>
    public const string BroadcastDisabledReason =
        "L'envoi du planning aux membres n'est pas encore disponible : ce message n'a pas de réglage de notification.";

    public const string PreviewDisabledReason =
        "L'aperçu du planning arrive dans un lot ultérieur.";

    /// <summary>
    /// What the foot of the week card says. The prototype writes « Dernière
    /// diffusion : dimanche dernier », but nothing has ever been broadcast and
    /// nothing records one — so it states where the feature stands instead of
    /// inventing a date.
    /// </summary>
    public const string BroadcastFootnote =
        "La diffusion aux membres n'est pas encore active. Le planning reste consultable ici.";
}
