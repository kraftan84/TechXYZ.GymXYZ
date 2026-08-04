using System.Globalization;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// French formats, spelled out once. Thousands separated by a space, decimal
/// comma, a narrow no-break space before the euro sign, 24-hour times, dates
/// written "9 juin 2026".
/// </summary>
public static class GxFormats
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>Narrow no-break space, the typographic rule before "€".</summary>
    private const string NarrowNoBreakSpace = " ";

    /// <summary>Whole number with a space every three digits: "1 200".</summary>
    public static string Count(int value) => value.ToString("N0", Culture);

    /// <summary>
    /// "1 membre" / "6 membres" — the plural mark follows the number, and in
    /// French zero stays singular.
    /// </summary>
    public static string Plural(int value, string singular, string plural)
        => $"{Count(value)} {(Math.Abs(value) < 2 ? singular : plural)}";

    /// <summary>Long date: "9 juin 2026".</summary>
    public static string Date(DateOnly date) => date.ToString("d MMMM yyyy", Culture);

    /// <summary>Short date: "09/06/2026".</summary>
    public static string ShortDate(DateOnly date) => date.ToString("dd/MM/yyyy", Culture);

    /// <summary>Membership month: "mars 2024".</summary>
    public static string Month(DateOnly date) => date.ToString("MMM yyyy", Culture);

    /// <summary>Day and time of a session: "lun. 9 juin · 09:00".</summary>
    public static string DayAndTime(DateTime moment) =>
        $"{moment.ToString("ddd d MMMM", Culture)} · {moment.ToString("HH:mm", Culture)}";

    public static string Time(DateTime moment) => moment.ToString("HH:mm", Culture);

    /// <summary>"49 €", with the narrow space the French rule asks for.</summary>
    public static string Amount(decimal amount) =>
        $"{amount.ToString("#,##0.##", Culture).Replace(' ', ' ')}{NarrowNoBreakSpace}€";

    /// <summary>
    /// How long ago, the way the prototype writes it: "aujourd'hui", "il y a 2 j",
    /// "il y a 3 sem.", then the month once it stops being useful.
    /// </summary>
    public static string SinceLabel(DateOnly date, DateOnly today)
    {
        var days = today.DayNumber - date.DayNumber;

        return days switch
        {
            <= 0 => "aujourd'hui",
            1 => "hier",
            < 7 => $"il y a {days} j",
            < 60 => $"il y a {days / 7} sem.",
            _ => Month(date)
        };
    }
}
