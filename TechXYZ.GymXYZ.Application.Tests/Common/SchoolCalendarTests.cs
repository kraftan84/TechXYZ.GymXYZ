using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The school-calendar logic ported from the prototype. It is pinned here rather
/// than through the service so it can be tested without calling two government
/// APIs — which is also why the zone table and the day lookup live in the
/// application layer and only the fetching lives in the web one.
/// </summary>
public class SchoolCalendarTests
{
    [Theory]
    [InlineData("69003", "A")]   // Lyon — the demo customer
    [InlineData("74100", "A")]
    [InlineData("59000", "B")]
    [InlineData("13001", "B")]
    [InlineData("75015", "C")]
    [InlineData("31000", "C")]
    public void ForPostcode_ShouldReadTheDepartment(string postcode, string expected)
    {
        SchoolZones.ForPostcode(postcode).ShouldBe(expected);
    }

    /// <summary>
    /// Anything unrecognised falls back to zone A rather than to nothing: the
    /// banner then says something slightly wrong instead of disappearing, which
    /// is what the prototype does.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("9")]
    [InlineData("97400")]
    public void ForPostcode_ShouldFallBackToZoneA(string? postcode)
    {
        SchoolZones.ForPostcode(postcode).ShouldBe(SchoolZones.DefaultZone);
    }

    /// <summary>A public holiday wins over school holidays — it is what closes the gym.</summary>
    [Fact]
    public void MarkFor_ShouldPreferThePublicHoliday()
    {
        var calendar = Calendar();

        calendar.MarkFor(new DateOnly(2026, 5, 1))!.Kind.ShouldBe(SchoolDayKind.PublicHoliday);
        calendar.MarkFor(new DateOnly(2026, 4, 20))!.Kind.ShouldBe(SchoolDayKind.SchoolVacation);
        calendar.MarkFor(new DateOnly(2026, 6, 10)).ShouldBeNull();
    }

    /// <summary>
    /// The last day off is the one before term restarts, so the restart date
    /// itself is an ordinary day.
    /// </summary>
    [Fact]
    public void MarkFor_ShouldStopOnTheLastDayOff()
    {
        var calendar = Calendar();

        calendar.MarkFor(new DateOnly(2026, 5, 3)).ShouldNotBeNull();
        calendar.MarkFor(new DateOnly(2026, 5, 4)).ShouldBeNull();
    }

    /// <summary>
    /// The banner lists one pill per event of the week, not one per day: this
    /// week is seven days of school holidays with a public holiday inside it,
    /// and it comes back as two pills.
    /// </summary>
    [Fact]
    public void MarksBetween_ShouldNotRepeatTheSameEvent()
    {
        var marks = Calendar().MarksBetween(new DateOnly(2026, 4, 27), new DateOnly(2026, 5, 3));

        marks.Select(mark => mark.Label).ShouldBe(["Vacances de printemps", "Fête du Travail"]);
    }

    [Fact]
    public void Outlook_ShouldReadWhatIsCurrentAndWhatIsNext()
    {
        var outlook = Calendar().Outlook(new DateOnly(2026, 4, 20));

        outlook.CurrentVacation!.Label.ShouldBe("Vacances de printemps");
        outlook.NextHoliday!.Label.ShouldBe("Fête du Travail");
        outlook.NextVacation.ShouldBeNull();
    }

    /// <summary>
    /// A calendar the sources would not answer for marks nothing at all. It is
    /// the case the planning has to survive, so it is pinned.
    /// </summary>
    [Fact]
    public void Unavailable_ShouldMarkNothing()
    {
        var calendar = SchoolCalendarDto.Unavailable("A");

        calendar.IsAvailable.ShouldBeFalse();
        calendar.MarkFor(new DateOnly(2026, 5, 1)).ShouldBeNull();
        calendar.MarksBetween(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).ShouldBeEmpty();
        calendar.Outlook(new DateOnly(2026, 5, 1)).ShouldBe(SchoolCalendarOutlookDto.Empty);
    }

    /// <summary>
    /// A customer who has turned the school holidays off keeps the public ones:
    /// a jour férié changes the opening hours of any gym, whoever its members
    /// are, and only the vacations were asked to go.
    /// </summary>
    [Fact]
    public void WithoutVacations_ShouldDropOnlyTheSchoolHolidays()
    {
        var calendar = Calendar().WithoutVacations();

        calendar.Vacations.ShouldBeEmpty();
        calendar.MarkFor(new DateOnly(2026, 4, 20)).ShouldBeNull();

        calendar.MarkFor(new DateOnly(2026, 5, 1))!.Kind.ShouldBe(SchoolDayKind.PublicHoliday);
        calendar.Outlook(new DateOnly(2026, 4, 1)).NextHoliday.ShouldNotBeNull();
    }

    /// <summary>
    /// Hiding them is a choice, not a source that would not answer. If the two
    /// ended up looking alike the banner would read "Calendrier indisponible" at
    /// a customer who had simply switched something off.
    /// </summary>
    [Fact]
    public void WithoutVacations_ShouldStayAvailable()
    {
        Calendar().WithoutVacations().IsAvailable.ShouldBeTrue();

        // And an unavailable calendar does not become available by being filtered.
        SchoolCalendarDto.Unavailable("A").WithoutVacations().IsAvailable.ShouldBeFalse();
    }

    private static SchoolCalendarDto Calendar() => new(
        "A",
        [new PublicHolidayDto(new DateOnly(2026, 5, 1), "Fête du Travail")],
        [new SchoolVacationDto("Vacances de printemps", new DateOnly(2026, 4, 11), new DateOnly(2026, 5, 3))]);
}
