namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The week as this product reads it: Monday first.
/// <para>
/// <see cref="DayOfWeek"/> numbers Sunday zero, which makes « samedi – dimanche »
/// look like a reversed range and would refuse a weekend opening every gym has.
/// Ordering questions go through here rather than comparing the enum.
/// </para>
/// </summary>
public static class WeekDays
{
    /// <summary>0 for Monday through 6 for Sunday.</summary>
    public static int Index(DayOfWeek day) => ((int)day + 6) % 7;

    /// <summary>Whether a day range reads forwards on a Monday-first week.</summary>
    public static bool IsForwardRange(DayOfWeek from, DayOfWeek to) => Index(to) >= Index(from);

    /// <summary>The days a range covers, ends included.</summary>
    public static IEnumerable<DayOfWeek> Between(DayOfWeek from, DayOfWeek to)
    {
        for (var index = Index(from); index <= Index(to); index++)
        {
            yield return (DayOfWeek)((index + 1) % 7);
        }
    }
}
