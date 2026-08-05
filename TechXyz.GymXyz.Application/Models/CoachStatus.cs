namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The standing shown on a coach card. Derived from the leave date, never
/// stored.
/// <para>
/// "Cours pleins" is a refinement of <see cref="Available"/>, not a third state
/// of its own: a coach whose classes fill up is still a coach you can book. It
/// is why the filter chips stay two, while the card can show three labels.
/// </para>
/// </summary>
public enum CoachStatus
{
    Available,

    /// <summary>"Cours pleins" — the sessions this coach runs come back nearly full.</summary>
    FullClasses,

    Away
}
