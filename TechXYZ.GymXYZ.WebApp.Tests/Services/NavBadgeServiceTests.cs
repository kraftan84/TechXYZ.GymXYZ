using Shouldly;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXYZ.GymXYZ.WebApp.Tests.Services;

/// <summary>
/// The two navigation counters. What matters is when the shells are told to
/// redraw, and that nought is no badge rather than a badge reading nought.
/// </summary>
public class NavBadgeServiceTests
{
    [Fact]
    public void BothCounters_ShouldStartUnknown()
    {
        var badge = new NavBadgeService();

        // Null, not zero: before anything has answered there is nothing to say,
        // and a zero would claim the work has been checked and found empty.
        badge.SheetsToPoint.ShouldBeNull();
        badge.SubscriptionsToWatch.ShouldBeNull();
    }

    [Fact]
    public void Counters_ShouldBeIndependent()
    {
        var badge = new NavBadgeService();

        badge.SetSheetsToPoint(3);

        badge.SheetsToPoint.ShouldBe(3);
        badge.SubscriptionsToWatch.ShouldBeNull();

        badge.SetSubscriptionsToWatch(6);

        badge.SheetsToPoint.ShouldBe(3);
        badge.SubscriptionsToWatch.ShouldBe(6);
    }

    [Fact]
    public void Zero_ShouldClearTheBadgeRatherThanDrawIt()
    {
        var badge = new NavBadgeService();
        badge.SetSheetsToPoint(2);

        badge.SetSheetsToPoint(0);

        badge.SheetsToPoint.ShouldBeNull();
    }

    [Fact]
    public void Changed_ShouldFireOnlyWhenTheFigureMoves()
    {
        var badge = new NavBadgeService();
        var redraws = 0;
        badge.Changed += () => redraws++;

        badge.SetSheetsToPoint(3);
        redraws.ShouldBe(1);

        // The Accueil publishes, then Présences publishes the same figure. The
        // shells must not be asked to redraw for news that is not news.
        badge.SetSheetsToPoint(3);
        redraws.ShouldBe(1);

        badge.SetSheetsToPoint(2);
        redraws.ShouldBe(2);

        badge.SetSubscriptionsToWatch(6);
        redraws.ShouldBe(3);
    }
}
