using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Planning;

/// <summary>
/// How the planning reads on screen: the labels of the seven columns, the hours
/// the grid draws, and the class and wording of a block. Static and stateless,
/// so the week grid, the day view and the mobile agenda all say the same thing.
/// </summary>
public static class PlanningFilters
{
    /// <summary>Monday first, as the grid is drawn.</summary>
    public static readonly IReadOnlyList<string> DayLabels =
        ["Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim"];

    public static readonly IReadOnlyList<string> LongDayLabels =
        ["Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche"];

    /// <summary>7:00 to 21:00, the span the prototype's grid covers.</summary>
    public static readonly IReadOnlyList<int> Hours =
        [.. Enumerable.Range(PlanningRules.FirstHour, PlanningRules.LastHour - PlanningRules.FirstHour + 1)];

    /// <summary>"Semaine du 3 août au 9 août 2026".</summary>
    public static string RangeLabel(DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        return $"Semaine du {GxFormats.DayAndMonth(weekStart)} au {GxFormats.DayAndMonth(weekEnd)} {weekEnd.Year}";
    }

    /// <summary>"03/08", under the day name in the sticky header.</summary>
    public static string DayNumberLabel(DateOnly day) => day.ToString("dd/MM", GxFormats.Culture);

    /// <summary>"Vendredi 7 août", the title of the day view.</summary>
    public static string LongDayLabel(DateOnly day) =>
        $"{LongDayLabels[((int)day.DayOfWeek + 6) % 7]} {GxFormats.DayAndMonth(day)}";

    /// <summary>
    /// The block's own class. Full turns it red, a private session greys it, and
    /// a cancelled one is greyed too — it is on the grid so the manager can see
    /// what they called off, not to be booked into.
    /// </summary>
    public static string EventClass(PlanningSessionDto session)
    {
        if (session.IsCancelled)
        {
            return "gx-ev priv";
        }

        return session.IsFull ? "gx-ev full" : session.IsPrivate ? "gx-ev priv" : "gx-ev";
    }

    /// <summary>"N. Lemoine · 14/20", "S. El Amrani · privé", or just the count.</summary>
    public static string MetaLine(PlanningSessionDto session)
    {
        var occupancy = session.IsCancelled
            ? "annulé"
            : session.IsPrivate
                ? "privé"
                : $"{session.Registered}/{session.Capacity}";

        return session.CoachShortName is { } coach ? $"{coach} · {occupancy}" : occupancy;
    }

    /// <summary>What the agenda card writes under the name: "N. Lemoine · 12/16 places".</summary>
    public static string AgendaMeta(PlanningSessionDto session)
    {
        var occupancy = session.IsCancelled
            ? "Annulé"
            : session.IsPrivate
                ? "Sur RDV"
                : $"{session.Registered}/{session.Capacity} places";

        return session.CoachShortName is { } coach ? $"{coach} · {occupancy}" : occupancy;
    }

    /// <summary>The gauge tone: amber once the class is filling, red when it is full.</summary>
    public static GxTone FillTone(PlanningSessionDto session) => session switch
    {
        { IsFull: true } => GxTone.Danger,
        _ when session.FillRate() >= PlanningRules.HighDemandThreshold => GxTone.Warning,
        _ => GxTone.Brand
    };

    /// <summary>Seats taken as a percentage, floored at nothing and capped at full.</summary>
    public static int FillRate(this PlanningSessionDto session) =>
        Math.Clamp(PlanningRules.FillRate(session.Registered, session.Capacity) ?? 0, 0, 100);

    /// <summary>"2 cours · 36 participants attendus", the day view's subtitle.</summary>
    public static string DaySummary(IReadOnlyList<PlanningSessionDto> sessions)
    {
        var expected = sessions.Where(session => !session.IsCancelled).Sum(session => session.Registered);

        return $"{GxFormats.Plural(sessions.Count, "cours", "cours")} · "
               + $"{GxFormats.Plural(expected, "participant attendu", "participants attendus")}";
    }
}
