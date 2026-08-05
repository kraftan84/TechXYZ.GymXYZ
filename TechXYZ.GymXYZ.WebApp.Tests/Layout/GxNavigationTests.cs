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

    /// <summary>
    /// The pre-hand-off /rooms screen was deleted with lot 4, so the entry has
    /// to point at the hand-off one — both shells read it from here.
    /// </summary>
    [Fact]
    public void Lieux_ShouldPointAtTheHandOffScreen()
    {
        GxNavigation.Lieux.Href.ShouldBe("/lieux");
        GxNavigation.Groups[3].Items.ShouldContain(GxNavigation.Lieux);
        GxNavigation.MobileMore.ShouldContain(GxNavigation.Lieux);
    }

    /// <summary>
    /// Same story as /rooms: the pre-hand-off /plannings screen went with the
    /// Lesson entities it was mocking, and both shells read the entry from here.
    /// </summary>
    [Fact]
    public void Planning_ShouldPointAtTheHandOffScreen()
    {
        GxNavigation.Planning.Href.ShouldBe("/planning");
        GxNavigation.Groups[0].Items.ShouldContain(GxNavigation.Planning);
        GxNavigation.MobileTabs.ShouldContain(GxNavigation.Planning);
    }

    [Fact]
    public void Visible_ShouldDropCoachs_FromTheMobilePlusSheet_ForASoloCoach()
    {
        var items = GxNavigation.Visible(GxNavigation.MobileMore, isSolo: true).ToList();

        items.ShouldNotContain(GxNavigation.Coachs);
        items.ShouldContain(GxNavigation.Reglages);
    }
}
