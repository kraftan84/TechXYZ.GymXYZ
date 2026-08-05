using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// How an attendance verdict is written and tinted, everywhere it appears.
/// <para>
/// The wordings are the prototype's own — "Présent", "Retard", "Absent", and
/// "En attente" for a seat nobody has reached. They live in one place because
/// the member record shows them today and the four Présences presentations will
/// show them next: a seat chipped "Retard" on one screen and "En retard" on
/// another would read as two different things.
/// </para>
/// </summary>
public static class AttendanceLabels
{
    public static string Label(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => "Présent",
        AttendanceStatus.Late => "Retard",
        AttendanceStatus.Absent => "Absent",
        _ => "En attente"
    };

    public static GxTone Tone(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => GxTone.Success,
        AttendanceStatus.Late => GxTone.Warning,
        AttendanceStatus.Absent => GxTone.Danger,
        _ => GxTone.Neutral
    };

    public static string Icon(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => GxIconPaths.Check,
        AttendanceStatus.Late => GxIconPaths.Clock,
        AttendanceStatus.Absent => GxIconPaths.Minus,
        _ => GxIconPaths.User
    };

    /// <summary>
    /// The three a coach can tap. <see cref="AttendanceStatus.Pending"/> is not
    /// among them: it is where a seat starts, not a verdict anybody chooses.
    /// </summary>
    public static readonly AttendanceStatus[] Choices =
    [
        AttendanceStatus.Present,
        AttendanceStatus.Late,
        AttendanceStatus.Absent
    ];
}
