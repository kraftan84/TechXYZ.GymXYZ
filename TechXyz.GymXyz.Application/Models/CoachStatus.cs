namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The standing shown on a coach card. Derived from the leave date, never
/// stored.
/// <para>
/// The prototype has a third value, "Cours pleins" (warning), which reads the
/// fill rate of the sessions a coach runs. Sessions land at lot 5 (Planning),
/// so that value is not produced here rather than being guessed.
/// </para>
/// </summary>
public enum CoachStatus
{
    Available,
    Away
}
