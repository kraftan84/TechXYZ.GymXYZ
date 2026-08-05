using System.Globalization;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The two strings the session DTOs carry ready-made: "09:00" and "Lun".
/// <para>
/// Lots 2 to 4 shaped <c>LocationSessionDto</c>, <c>CourseSessionDto</c> and
/// <c>CoachSessionDto</c> around formatted strings rather than dates, so the
/// formatting has to happen here. It is deliberately the plain form the screens
/// show, not a localisable one — the product ships in French only.
/// </para>
/// </summary>
public static class SessionLabels
{
    private static readonly string[] ShortDays = ["Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim"];

    /// <summary>"09:00".</summary>
    public static string Time(DateTime moment) => moment.ToString("HH:mm", CultureInfo.InvariantCulture);

    /// <summary>"Ven", Monday first.</summary>
    public static string ShortDay(DateTime moment) => ShortDays[((int)moment.DayOfWeek + 6) % 7];
}
