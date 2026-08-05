using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Shared;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Presences;

/// <summary>
/// Where a sheet stands, as the screen says it. Derived from the session, never
/// stored: <see cref="Pointed"/> is <c>AttendanceClosedAt</c> being set, and
/// <see cref="Live"/> is the clock falling inside the slot.
/// </summary>
public enum PresenceState
{
    ToPoint,
    Live,
    Pointed
}

/// <summary>
/// The wordings, tones and tallies of the Présences screens. Static so that the
/// four presentations — two desktop, two mobile — cannot end up saying different
/// things about the same sheet.
/// <para>
/// Desktop and mobile wordings differ where the prototype makes them differ, and
/// each is the prototype's own.
/// </para>
/// </summary>
public static class PresenceFilters
{
    public static PresenceState StateOf(AttendanceSessionDto session, DateTime now) =>
        session.IsPointed ? PresenceState.Pointed
            : session.IsLive(now) ? PresenceState.Live
            : PresenceState.ToPoint;

    /// <summary>Desktop chip: « Pointé » · « En cours » · « À pointer ».</summary>
    public static string Label(PresenceState state) => state switch
    {
        PresenceState.Pointed => "Pointé",
        PresenceState.Live => "En cours",
        _ => "À pointer"
    };

    public static GxTone Tone(PresenceState state) => state switch
    {
        PresenceState.Pointed => GxTone.Success,
        PresenceState.Live => GxTone.Warning,
        _ => GxTone.Brand
    };

    public static string? Icon(PresenceState state) => state switch
    {
        PresenceState.Pointed => GxIconPaths.Check,
        PresenceState.Live => GxIconPaths.Clock,
        _ => null
    };

    /// <summary>
    /// Mobile draws "En cours" solid red and "À pointer" in warning, where the
    /// desktop uses brand — the prototype's own difference, kept.
    /// </summary>
    public static GxTone MobileTone(PresenceState state) => state switch
    {
        PresenceState.Pointed => GxTone.Success,
        PresenceState.Live => GxTone.Danger,
        _ => GxTone.Warning
    };

    /// <summary>"12 inscrits" · "5/12 arrivés" · "8/12 présents", by state.</summary>
    public static string Tally(AttendanceSessionDto session, PresenceState state) => state switch
    {
        PresenceState.Pointed => $"{session.Attended}/{session.Registered} présents",
        PresenceState.Live => $"{session.Attended}/{session.Registered} arrivés",
        _ => GxFormats.Plural(session.Registered, "inscrit", "inscrits")
    };

    /// <summary>"Studio A · Nora Lemoine", skipping whatever is absent.</summary>
    public static string Meta(AttendanceSessionDto session, bool withDay = false)
    {
        var parts = new List<string> { session.LocationName };

        if (session.CoachFullName is { } coach)
        {
            parts.Add(coach);
        }

        if (withDay && session.StartsAt.Date != DateTime.Today)
        {
            parts.Add(DayLabel(session.StartsAt));
        }

        return string.Join(" · ", parts);
    }

    /// <summary>"Aujourd'hui", "Hier", then the plain date.</summary>
    public static string DayLabel(DateTime moment)
    {
        var days = (DateTime.Today - moment.Date).Days;

        return days switch
        {
            0 => "Aujourd'hui",
            1 => "Hier",
            _ => GxFormats.DayAndMonth(DateOnly.FromDateTime(moment))
        };
    }

    /// <summary>"— " rather than "0 %" when nothing on the sheet was pointed.</summary>
    public static string Rate(int? rate) => rate is { } value ? $"{value} %" : "—";

    /// <summary>
    /// "3 séances à pointer" under the page title, or what is left to do when
    /// there is nothing.
    /// </summary>
    public static string Subtitle(AttendanceOverviewDto overview) =>
        overview.ToPoint.Count == 0
            ? $"{GxFormats.Date(overview.Day)} · tout est pointé"
            : $"{GxFormats.Date(overview.Day)} · {GxFormats.Plural(overview.ToPoint.Count, "séance à pointer", "séances à pointer")}";

    /// <summary>"5 absences / 8 · dernière venue il y a 3 sem." on the chase card.</summary>
    public static string ChaseMeta(AbsentMemberDto member)
    {
        var missed = $"{GxFormats.Plural(member.Missed, "absence", "absences")} / {member.Booked}";

        return member.LastVisitOn is { } last
            ? $"{missed} · dernière venue {Ago(last)}"
            : $"{missed} · jamais venu·e";
    }

    /// <summary>"il y a 3 j", "il y a 2 sem.", "il y a 3 mois".</summary>
    public static string Ago(DateTime moment)
    {
        var days = Math.Max(0, (DateTime.Today - moment.Date).Days);

        return days switch
        {
            0 => "aujourd'hui",
            1 => "hier",
            < 14 => $"il y a {days} j",
            < 60 => $"il y a {days / 7} sem.",
            _ => $"il y a {days / 30} mois"
        };
    }

    /// <summary>
    /// The bar of a course is tinted when the course is falling behind — the
    /// prototype's own threshold on this card.
    /// </summary>
    public const int LowAttendanceThreshold = 75;

    public static GxTone BarTone(int rate) =>
        rate < LowAttendanceThreshold ? GxTone.Warning : GxTone.Brand;
}
