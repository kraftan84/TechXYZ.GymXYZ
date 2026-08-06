using Shouldly;
using TechXyz.GymXyz.Application.Common;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class WeekDaysTests
{
    [Fact]
    public void Index_ShouldStartTheWeekOnMonday()
    {
        WeekDays.Index(DayOfWeek.Monday).ShouldBe(0);
        WeekDays.Index(DayOfWeek.Sunday).ShouldBe(6);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, DayOfWeek.Friday, true)]
    [InlineData(DayOfWeek.Saturday, DayOfWeek.Sunday, true)]
    [InlineData(DayOfWeek.Sunday, DayOfWeek.Sunday, true)]
    [InlineData(DayOfWeek.Sunday, DayOfWeek.Monday, false)]
    [InlineData(DayOfWeek.Friday, DayOfWeek.Tuesday, false)]
    public void IsForwardRange_ShouldReadTheWeekMondayFirst(DayOfWeek from, DayOfWeek to, bool expected)
    {
        // Saturday–Sunday is the one every gym has, and the one a plain
        // DayOfWeek comparison would refuse.
        WeekDays.IsForwardRange(from, to).ShouldBe(expected);
    }

    [Fact]
    public void Between_ShouldWalkTheRangeEndsIncluded()
    {
        WeekDays.Between(DayOfWeek.Monday, DayOfWeek.Wednesday)
            .ShouldBe([DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday]);

        WeekDays.Between(DayOfWeek.Saturday, DayOfWeek.Sunday)
            .ShouldBe([DayOfWeek.Saturday, DayOfWeek.Sunday]);

        WeekDays.Between(DayOfWeek.Sunday, DayOfWeek.Sunday).ShouldBe([DayOfWeek.Sunday]);
    }
}
