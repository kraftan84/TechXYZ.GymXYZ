namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What every screen reading sessions has to agree on: where a week starts,
/// which hours the grid draws, and when a fill rate counts as high demand.
/// Spelled out once because the planning, the venue heatmap, the course
/// catalogue and the coach cards all answer the same questions.
/// </summary>
public static class PlanningRules
{
    /// <summary>First hour row of the grid.</summary>
    public const int FirstHour = 7;

    /// <summary>Last hour row of the grid.</summary>
    public const int LastHour = 21;

    /// <summary>
    /// At and above this fill, a venue reads "Forte demande" and a coach reads
    /// "Cours pleins". One threshold, so the two chips never disagree about what
    /// busy means.
    /// </summary>
    public const int HighDemandThreshold = 90;

    /// <summary>The Monday of the week the date falls in.</summary>
    public static DateOnly MondayOf(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    /// <summary>The Monday of the week the moment falls in, at midnight.</summary>
    public static DateTime MondayOf(DateTime moment) =>
        moment.Date.AddDays(-(((int)moment.DayOfWeek + 6) % 7));

    /// <summary>
    /// A percentage of seats taken, or null when there was nothing to fill —
    /// zero sessions is not zero percent, and the screens show "—" for it.
    /// </summary>
    public static int? FillRate(int registered, int capacity) =>
        capacity <= 0 ? null : (int)Math.Round(100d * registered / capacity);
}
