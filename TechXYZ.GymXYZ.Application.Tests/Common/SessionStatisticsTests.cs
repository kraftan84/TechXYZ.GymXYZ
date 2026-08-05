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
    public void AttendanceRate_ShouldCountLateArrivalsAsAttended()
    {
        // 7 of the 8 seats that were pointed had the member in the room.
        var rate = SessionStatistics.AttendanceRate([Fact(20, 8, present: 6, late: 1, absent: 1)]);

        rate.ShouldBe(88);
    }

    [Fact]
    public void AttendanceRate_ShouldBeNullWhenNothingWasMarked()
    {
        // A full session whose sheet nobody opened. Nought per cent would be a
        // verdict; there is none.
        var rate = SessionStatistics.AttendanceRate([Fact(20, 20), Fact(16, 12)]);

        rate.ShouldBeNull();
    }

    [Fact]
    public void AttendanceRate_ShouldIgnoreTheSeatsStillPending()
    {
        // Half-pointed sheet: 4 marked, 3 of them attended. The 16 seats nobody
        // reached must not drag it down.
        var rate = SessionStatistics.AttendanceRate([Fact(20, 20, present: 3, absent: 1)]);

        rate.ShouldBe(75);
    }

    [Fact]
    public void AttendanceRate_ShouldPoolTheSessionsRatherThanAverageTheirRates()
    {
        // A twenty-seat class and a one-to-one do not weigh the same. Pooling
        // gives 19/21; averaging the two rates would give 50.
        var rate = SessionStatistics.AttendanceRate(
        [
            Fact(20, 20, present: 19, absent: 1),
            Fact(1, 1, absent: 1)
        ]);

        rate.ShouldBe(90);
    }

    [Fact]
    public void AttendanceWindow_ShouldRunBackAQuarter()
    {
        var now = DateTime.Today.AddHours(15);

        var (from, to) = SessionStatistics.AttendanceWindow(now);

        from.ShouldBe(DateTime.Today.AddDays(-SessionStatistics.AttendanceWindowDays));
        to.ShouldBe(now);
    }

    [Fact]
    public void CurrentWeek_ShouldRunMondayToMonday()
    {
        var (from, to) = SessionStatistics.CurrentWeek(DateTime.Today.AddHours(15));

        from.DayOfWeek.ShouldBe(DayOfWeek.Monday);
        from.TimeOfDay.ShouldBe(TimeSpan.Zero);
        to.ShouldBe(from.AddDays(7));
    }

    private static SessionFact Fact(
        int capacity,
        int registered,
        DateTime? startsAt = null,
        int present = 0,
        int late = 0,
        int absent = 0,
        DateTime? closedAt = null) =>
        new(0, 0, null, 0, startsAt ?? DateTime.Today, capacity, registered, present, late, absent, closedAt);
}
