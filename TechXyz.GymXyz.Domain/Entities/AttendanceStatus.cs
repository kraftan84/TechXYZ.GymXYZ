namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What the attendance sheet recorded for one seat.
/// <para>
/// <see cref="Pending"/> is the state every seat starts in and is not a verdict:
/// it means nobody has said yet. That distinction is what keeps a sheet nobody
/// pointed from reading as nought per cent attendance.
/// </para>
/// </summary>
public enum AttendanceStatus
{
    /// <summary>Not pointed yet.</summary>
    Pending,

    Present,

    /// <summary>There, but after the session started. Counts as attended.</summary>
    Late,

    Absent
}
