namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Where a session stands. <see cref="Scheduled"/> and <see cref="Cancelled"/>
/// are written by the planning; <see cref="Live"/> and <see cref="Done"/> follow
/// the clock and the attendance sheet, which arrives at lot 6.
/// </summary>
public enum SessionStatus
{
    Scheduled,
    Live,
    Done,
    Cancelled
}
