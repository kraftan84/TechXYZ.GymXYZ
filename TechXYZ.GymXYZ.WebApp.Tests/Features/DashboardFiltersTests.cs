using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Features.Dashboard;
using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

/// <summary>
/// What the Accueil says about the figures it is handed. Both presentations read
/// this, so a wording decided here cannot differ between the two shells.
/// </summary>
public class DashboardFiltersTests
{
    [Fact]
    public void Alerts_ShouldKeepOnlyWhatHasSomethingToReport()
    {
        var quiet = DashboardFilters.Alerts(new DashboardAlertsDto(0, 0, 0m, 0));

        // "0 abonnements expirent" would be a row reporting work where there is
        // none. The card says nothing rather than saying nought three times.
        quiet.ShouldBeEmpty();

        var busy = DashboardFilters.Alerts(new DashboardAlertsDto(4, 2, 180m, 3));
        busy.Count.ShouldBe(3);
    }

    [Fact]
    public void Alerts_ShouldKeepThePrototypeOrder()
    {
        var alerts = DashboardFilters.Alerts(new DashboardAlertsDto(4, 2, 180m, 3));

        alerts.Select(alert => alert.Action).ShouldBe(["Relancer", "Voir", "Pointer"]);
        alerts[0].Tone.ShouldBe(GxTone.Warning);
        alerts[1].Tone.ShouldBe(GxTone.Danger);
        alerts[2].Tone.ShouldBe(GxTone.Brand);
    }

    [Fact]
    public void Alerts_ShouldAgreeWithThemselvesOnNumber()
    {
        var one = DashboardFilters.Alerts(new DashboardAlertsDto(1, 1, 49m, 1));

        one[0].Title.ShouldBe("1 abonnement expire");
        one[1].Title.ShouldBe("1 paiement en retard");

        var many = DashboardFilters.Alerts(new DashboardAlertsDto(4, 2, 180m, 3));

        many[0].Title.ShouldBe("4 abonnements expirent");
        many[1].Title.ShouldBe("2 paiements en retard");
    }

    /// <summary>
    /// The prototype titles the third alert « Présences d'hier ». The rule behind
    /// it reaches a week back, so a sheet forgotten on Friday sits under it on
    /// Monday — the title says what is counted instead of when it happened.
    /// </summary>
    [Fact]
    public void Alerts_ShouldNotClaimTheSheetsAreYesterdays()
    {
        var alerts = DashboardFilters.Alerts(new DashboardAlertsDto(0, 0, 0m, 3));

        alerts.ShouldHaveSingleItem();
        alerts[0].Title.ShouldBe("Présences à pointer");
        alerts[0].Detail.ShouldBe("3 cours à pointer");
    }

    [Fact]
    public void Alerts_ShouldLeadToTheScreenThatWorksThem()
    {
        var alerts = DashboardFilters.Alerts(new DashboardAlertsDto(4, 2, 180m, 3));

        alerts[0].Href.ShouldBe("/abonnements");
        alerts[1].Href.ShouldBe("/abonnements");
        alerts[2].Href.ShouldBe("/presences");
    }

    [Fact]
    public void WeekMeta_ShouldDropTheCoachesForASoloGym()
    {
        var dashboard = DashboardWith(sessions: 28, coaches: 6);

        DashboardFilters.WeekMeta(dashboard, isSolo: false).ShouldBe("28 cours · 6 coachs");

        // A solo coach has no team to count, and « · 1 coach » would be telling
        // them something they already know about themselves.
        DashboardFilters.WeekMeta(dashboard, isSolo: true).ShouldBe("28 cours");
    }

    [Fact]
    public void WeekMeta_ShouldDropTheCoachesWhenNobodyIsOn()
    {
        DashboardFilters.WeekMeta(DashboardWith(sessions: 3, coaches: 0), isSolo: false)
            .ShouldBe("3 cours");
    }

    [Fact]
    public void TodayTitle_ShouldCountTheDaysClasses()
    {
        DashboardFilters.TodayTitle(DashboardWith(sessions: 0, coaches: 0, today: 0))
            .ShouldBe("Aujourd'hui · 0 cours");

        DashboardFilters.TodayTitle(DashboardWith(sessions: 9, coaches: 2, today: 3))
            .ShouldBe("Aujourd'hui · 3 cours");
    }

    private static DashboardDto DashboardWith(int sessions, int coaches, int today = 0)
    {
        var monday = new DateOnly(2026, 8, 3);

        // The whole week's classes on the Monday cell: this fixture is about the
        // wording, and WeekSessionCount reads the strip.
        var week = Enumerable.Range(0, 7)
            .Select(offset => new DashboardDayDto(
                monday.AddDays(offset),
                offset == 0 ? sessions : 0,
                offset == 0))
            .ToList();

        var classes = Enumerable.Range(0, today)
            .Select(index => new DashboardClassDto(
                index,
                monday.ToDateTime(new TimeOnly(9 + index, 0)),
                monday.ToDateTime(new TimeOnly(10 + index, 0)),
                $"Cours {index}",
                null,
                null,
                "Studio A",
                4,
                16))
            .ToList();

        return new DashboardDto(monday, monday, week, classes, DashboardAlertsDto.Empty, coaches);
    }
}
