namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One week as it may be <b>published</b> — the content of the image the manager
/// posts on Instagram or prints.
/// <para>
/// Deliberately not <see cref="WeekPlanningDto"/>, though it describes the same
/// seven days. The poster is public, and the rule for it is that no member's
/// name, no enrolled headcount and no price may appear. Written as a separate
/// shape, that rule stops being a discipline somebody has to remember at each
/// screen edit: the fields simply are not here. There is no capacity beside
/// <see cref="PosterSessionDto.RemainingSeats"/> either, because capacity minus
/// remaining is the headcount, and publishing both would leak by subtraction
/// what neither leaks alone.
/// </para>
/// </summary>
public sealed record PosterWeekDto(DateOnly WeekStart, IReadOnlyList<PosterSessionDto> Sessions)
{
    public static PosterWeekDto Empty { get; } = new(default, []);

    public DateOnly WeekEnd => WeekStart.AddDays(6);

    /// <summary>What the header counts — « 12 cours cette semaine ».</summary>
    public int CourseCount => Sessions.Count;

    /// <summary>The classes of one day, earliest first, as the row is drawn.</summary>
    public IReadOnlyList<PosterSessionDto> On(DateOnly day) =>
        Sessions.Where(session => DateOnly.FromDateTime(session.StartsAt) == day).ToList();

    /// <summary>
    /// The busiest day of the week. The poster gives every day the same number
    /// of columns so they stay aligned, so this is what that number is read
    /// from.
    /// </summary>
    public int BusiestDay =>
        Sessions.Count == 0
            ? 0
            : Sessions.GroupBy(session => session.StartsAt.Date).Max(day => day.Count());
}

/// <summary>
/// One cell of the poster: when, what, where, and how much room is left.
/// </summary>
public sealed record PosterSessionDto(
    DateTime StartsAt,
    int DurationMinutes,
    string CourseName,
    string LocationName,
    string? CoachShortName,
    int RemainingSeats,
    bool IsFull);
