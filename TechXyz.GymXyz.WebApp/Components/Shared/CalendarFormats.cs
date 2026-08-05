using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// How the school calendar reads on screen. Static and stateless, so the
/// planning banner and the settings card write the same words.
/// </summary>
public static class CalendarFormats
{
    /// <summary>The tint of a pill or of a day column: azure for a public holiday, amber for school holidays.</summary>
    public static string MarkClass(SchoolDayKind kind) =>
        kind == SchoolDayKind.PublicHoliday ? "ferie" : "vac";

    public static string MarkIcon(SchoolDayKind kind) =>
        kind == SchoolDayKind.PublicHoliday ? GxIconPaths.Star : GxIconPaths.Sun;

    /// <summary>
    /// What the banner says on a week nothing falls in: "Rien cette semaine —
    /// prochain férié : Assomption (15 août)".
    /// </summary>
    public static string OutlookSentence(SchoolCalendarOutlookDto outlook)
    {
        var bits = new List<string>();

        if (outlook.CurrentVacation is { } current)
        {
            bits.Add($"en cours : {current.Label.ToLower(GxFormats.Culture)}");
        }

        if (outlook.NextHoliday is { } holiday)
        {
            bits.Add($"prochain férié : {holiday.Label} ({GxFormats.DayAndMonth(holiday.Date)})");
        }

        if (outlook is { CurrentVacation: null, NextVacation: { } next })
        {
            bits.Add($"prochaines vacances : {next.Label.ToLower(GxFormats.Culture)} dès le {GxFormats.DayAndMonth(next.Start)}");
        }

        return bits.Count == 0
            ? "Rien cette semaine — aucun évènement à venir"
            : $"Rien cette semaine — {string.Join(" · ", bits)}";
    }

    /// <summary>The settings card's two lines: what is next, written out.</summary>
    public static string NextHolidayLabel(SchoolCalendarOutlookDto outlook) =>
        outlook.NextHoliday is { } holiday
            ? $"{holiday.Label} · {GxFormats.DayAndMonth(holiday.Date)}"
            : "—";

    public static string VacationLabel(SchoolCalendarOutlookDto outlook)
    {
        if (outlook.CurrentVacation is { } current)
        {
            return $"En cours : {current.Label} (jusqu'au {GxFormats.DayAndMonth(current.End)})";
        }

        return outlook.NextVacation is { } next
            ? $"{next.Label} dès le {GxFormats.DayAndMonth(next.Start)}"
            : "—";
    }
}
