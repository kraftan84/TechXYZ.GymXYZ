namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The bounds a course template stays inside, written once so create and update
/// cannot drift apart. The wordings are what the user reads on the drawer.
/// </summary>
public static class CourseTemplateRules
{
    public const int MinimumDurationMinutes = 5;
    public const int MaximumDurationMinutes = 300;

    /// <summary>One seat is the private lesson; there is no zero-seat course.</summary>
    public const int MinimumCapacity = 1;
    public const int MaximumCapacity = 200;

    public static readonly string DurationMessage =
        $"La durée doit être comprise entre {MinimumDurationMinutes} et {MaximumDurationMinutes} minutes.";

    public static readonly string CapacityMessage =
        $"La capacité doit être comprise entre {MinimumCapacity} et {MaximumCapacity} places.";
}
