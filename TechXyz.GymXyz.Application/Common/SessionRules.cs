namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The bounds a session stays inside, and the wordings the planning drawer shows
/// when one is crossed. Written once so creating and editing cannot drift apart.
/// </summary>
public static class SessionRules
{
    /// <summary>One occurrence, or a term's worth of them.</summary>
    public const int MinimumRecurrenceWeeks = 1;
    public const int MaximumRecurrenceWeeks = 52;

    public const int MinimumCapacity = 1;
    public const int MaximumCapacity = 200;

    public const int MaximumCancellationReasonLength = 500;

    public static readonly string EndsAfterStartMessage =
        "La séance doit se terminer après son début.";

    public static readonly string RecurrenceMessage =
        $"La répétition doit être comprise entre {MinimumRecurrenceWeeks} et {MaximumRecurrenceWeeks} semaines.";

    public static readonly string CapacityMessage =
        $"La capacité doit être comprise entre {MinimumCapacity} et {MaximumCapacity} places.";

    /// <summary>Invariant 1: a session never seats more than the venue holds.</summary>
    public static string OverCapacityMessage(string locationName, int capacity) =>
        $"{locationName} ne peut accueillir que {capacity} personnes.";

    /// <summary>Invariant 2: two sessions of the same venue never overlap.</summary>
    public static string LocationBusyMessage(string locationName, string when) =>
        $"{locationName} est déjà occupé {when}.";

    /// <summary>Invariant 3: a coach is never on two sessions at once.</summary>
    public static string CoachBusyMessage(string coachName, string when) =>
        $"{coachName} anime déjà une séance {when}.";
}
