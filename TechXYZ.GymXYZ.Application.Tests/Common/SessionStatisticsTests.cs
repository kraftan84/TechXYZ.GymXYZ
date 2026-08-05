using Shouldly;
using TechXyz.GymXyz.Application.Common;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// What every occupancy figure in the application is counted from. The rules
/// are pinned here because four screens read them and none of them may disagree
/// about what a busy week means.
/// </summary>
public class SessionStatisticsTests
{
    [Fact]
    public void FillRate_ShouldAverageOverSeatsNotOverSessions()
    {
        // 12 of 16 and 4 of 20: seat-weighted this is 16/36, not the 45 % a
        // mean of the two percentages would give.
        var rate = SessionStatistics.FillRate([Fact(capacity: 16, registered: 12), Fact(capacity: 20, registered: 4)]);

        rate.ShouldBe(44);
    }

    /// <summary>
    /// A session that seats one is full the moment it is booked. Counting it
    /// would make every coach who takes individual clients read "Cours pleins"
    /// and would flatter the studio they use.
    /// </summary>
    [Fact]
    public void FillRate_ShouldIgnorePrivateSessions()
    {
        var withPrivate = SessionStatistics.FillRate([
            Fact(capacity: 18, registered: 16),
            Fact(capacity: 1, registered: 1)
        ]);

        withPrivate.ShouldBe(89);
    }

    /// <summary>Nothing to fill is not an empty room: the screens show "—".</summary>
    [Fact]
    public void FillRate_ShouldBeNull_WhenThereIsNothingToMeasure()
    {
        SessionStatistics.FillRate([]).ShouldBeNull();
        SessionStatistics.FillRate([Fact(capacity: 1, registered: 1)]).ShouldBeNull();
    }

    /// <summary>
    /// The heatmap keeps seven cells whatever the week looks like — a day
    /// without a session reads zero rather than dropping out of the row.
    /// </summary>
    [Fact]
    public void DailyRates_ShouldAlwaysReturnSevenValues_MondayFirst()
    {
        var monday = PlanningRules.MondayOf(DateTime.Today);

        var rates = SessionStatistics.DailyRates([
            Fact(capacity: 20, registered: 10, startsAt: monday.AddHours(9)),
            Fact(capacity: 20, registered: 20, startsAt: monday.AddDays(3).AddHours(18))
        ]);

        rates.Count.ShouldBe(7);
        rates[0].ShouldBe(50);
        rates[3].ShouldBe(100);
        rates[6].ShouldBe(0);
    }

    [Fact]
    public void CurrentWeek_ShouldRunMondayToMonday()
    {
        var (from, to) = SessionStatistics.CurrentWeek(DateTime.Today.AddHours(15));

        from.DayOfWeek.ShouldBe(DayOfWeek.Monday);
        from.TimeOfDay.ShouldBe(TimeSpan.Zero);
        to.ShouldBe(from.AddDays(7));
    }

    private static SessionFact Fact(int capacity, int registered, DateTime? startsAt = null) =>
        new(0, 0, null, 0, startsAt ?? DateTime.Today, capacity, registered);
}
