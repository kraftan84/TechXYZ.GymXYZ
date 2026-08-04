using Shouldly;
using TechXyz.GymXyz.WebApp.Components.Layout;

namespace TechXYZ.GymXYZ.WebApp.Tests.Layout;

public class GxNavigationTests
{
    [Fact]
    public void Visible_ShouldKeepCoachs_ForARegularGym()
    {
        var items = GxNavigation.Visible(GxNavigation.Groups[1].Items, isSolo: false).ToList();

        items.ShouldContain(GxNavigation.Coachs);
    }

    [Fact]
    public void Visible_ShouldDropCoachs_ForASoloCoach()
    {
        var items = GxNavigation.Visible(GxNavigation.Groups[1].Items, isSolo: true).ToList();

        items.ShouldNotContain(GxNavigation.Coachs);
        items.ShouldContain(GxNavigation.Membres);
    }

    [Fact]
    public void Visible_ShouldDropCoachs_FromTheMobilePlusSheet_ForASoloCoach()
    {
        var items = GxNavigation.Visible(GxNavigation.MobileMore, isSolo: true).ToList();

        items.ShouldNotContain(GxNavigation.Coachs);
        items.ShouldContain(GxNavigation.Reglages);
    }
}
