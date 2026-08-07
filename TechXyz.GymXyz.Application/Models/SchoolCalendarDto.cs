namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Public holidays and school holidays for one zone, as the planning banner and
/// the settings card read them.
/// <para>
/// <see cref="IsAvailable"/> false is a normal outcome, not an error: the two
/// open data sources are outside the application, and a planning that refuses to
/// render because a government API is down would be a worse screen than one
/// without its banner.
/// </para>
/// </summary>
public sealed record SchoolCalendarDto(
    string Zone,
    IReadOnlyList<PublicHolidayDto> Holidays,
    IReadOnlyList<SchoolVacationDto> Vacations)
{
    public bool IsAvailable { get; init; } = true;

    public static SchoolCalendarDto Unavailable(string zone) =>
        new(zone, [], []) { IsAvailable = false };

    /// <summary>
    /// The same calendar with the school holidays dropped, for a customer who
    /// has turned them off. The public holidays stay: they close the gym whoever
    /// its members are, and the two are separate marks precisely so one can go
    /// without the other.
    /// <para>
    /// <see cref="IsAvailable"/> is carried over untouched — hiding the holidays
    /// is a choice, and it must not end up reading like a source that is down.
    /// </para>
    /// </summary>
    public SchoolCalendarDto WithoutVacations() =>
        this with { Vacations = [] };

    /// <summary>
    /// What marks a day on the grid, or null for an ordinary one. A public
    /// holiday wins over school holidays — it is the stronger signal, and the
    /// one that closes the gym.
    /// </summary>
    public SchoolDayMarkDto? MarkFor(DateOnly day)
    {
        if (!IsAvailable)
        {
            return null;
        }

        var holiday = Holidays.FirstOrDefault(candidate => candidate.Date == day);
        if (holiday is not null)
        {
            return new SchoolDayMarkDto(SchoolDayKind.PublicHoliday, holiday.Label);
        }

        var vacation = Vacations.FirstOrDefault(candidate => candidate.Covers(day));

        return vacation is null
            ? null
            : new SchoolDayMarkDto(SchoolDayKind.SchoolVacation, vacation.Label);
    }

    /// <summary>
    /// The marks falling inside a span, one per label — the banner lists what
    /// concerns the week shown, not one pill per day.
    /// </summary>
    public IReadOnlyList<SchoolDayMarkDto> MarksBetween(DateOnly from, DateOnly to)
    {
        var marks = new List<SchoolDayMarkDto>();

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            if (MarkFor(day) is { } mark && !marks.Contains(mark))
            {
                marks.Add(mark);
            }
        }

        return marks;
    }

    /// <summary>What is coming, for the weeks the banner has nothing to show.</summary>
    public SchoolCalendarOutlookDto Outlook(DateOnly from)
    {
        if (!IsAvailable)
        {
            return SchoolCalendarOutlookDto.Empty;
        }

        return new SchoolCalendarOutlookDto(
            Holidays
                .Where(holiday => holiday.Date >= from)
                .MinBy(holiday => holiday.Date),
            Vacations.FirstOrDefault(vacation => vacation.Covers(from)),
            Vacations
                .Where(vacation => vacation.Start > from)
                .MinBy(vacation => vacation.Start));
    }
}

public sealed record PublicHolidayDto(DateOnly Date, string Label);

/// <summary>
/// One school holiday span. <see cref="End"/> is the last day off, not the day
/// term restarts — the source publishes the restart date and the port shifts it
/// back a day, as the prototype does.
/// </summary>
public sealed record SchoolVacationDto(string Label, DateOnly Start, DateOnly End)
{
    public bool Covers(DateOnly day) => day >= Start && day <= End;
}

public sealed record SchoolDayMarkDto(SchoolDayKind Kind, string Label);

public enum SchoolDayKind
{
    PublicHoliday,
    SchoolVacation
}

public sealed record SchoolCalendarOutlookDto(
    PublicHolidayDto? NextHoliday,
    SchoolVacationDto? CurrentVacation,
    SchoolVacationDto? NextVacation)
{
    public static SchoolCalendarOutlookDto Empty { get; } = new(null, null, null);
}
