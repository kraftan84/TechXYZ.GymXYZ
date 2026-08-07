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
    public void ShortLabel_ShouldShortenOnlyTheTwoThatOverflowATile()
    {
        GxNavigation.Abonnements.ShortLabel.ShouldBe("Abos");
        GxNavigation.Administration.ShortLabel.ShouldBe("Admin.");
    }

    [Fact]
    public void ShortLabel_ShouldFallBackToTheFullWording()
    {
        // Only the long ones carry a second wording; everything else answers
        // with its own label, so the markup never has to test for a null.
        foreach (var item in GxNavigation.MobileMore.Except([GxNavigation.Abonnements, GxNavigation.Administration]))
        {
            item.ShortLabel.ShouldBe(item.Label);
        }
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

    [Fact]
    public void Visible_ShouldOfferNoBusinessSection_WithoutACustomer()
    {
        // A platform admin who has entered nobody. Every business screen would
        // query the ambient tenant — 0 — and come back empty, which reads as a
        // customer with no data rather than as no customer.
        var everything = GxNavigation.Groups
            .SelectMany(group => group.Items)
            .Concat(GxNavigation.MobileMore)
            .Distinct();

        var items = GxNavigation.Visible(everything, isSolo: false, hasCustomer: false).ToList();

        items.ShouldBe([GxNavigation.Administration]);
    }

    [Fact]
    public void Visible_ShouldDropReglages_WithoutACustomer()
    {
        // Réglages is the customer's own settings — its identity, its team, its
        // e-mail. Named on its own because it sits in the footer rather than in
        // a group, and is the one business entry easy to forget.
        GxNavigation.Visible([GxNavigation.Reglages], isSolo: false, hasCustomer: false)
            .ShouldBeEmpty();
    }

    [Fact]
    public void Visible_ShouldChangeNothing_ForEveryOtherRole()
    {
        // The half of this change that could quietly take access away from a
        // manager or a coach: anybody inside a customer must see exactly what
        // they saw before, which is what the default overload keeps promising.
        foreach (var solo in new[] { true, false })
        {
            var items = GxNavigation.Groups.SelectMany(group => group.Items);

            GxNavigation.Visible(items, solo, hasCustomer: true)
                .ShouldBe(GxNavigation.Visible(items, solo));
        }
    }

    [Fact]
    public void MobileTabsFor_ShouldReplaceTheFourTabs_WithTheConsole()
    {
        // Not an empty bar: all four tabs are about a customer, and a tab bar
        // holding nothing but "Plus" reads as a broken shell.
        GxNavigation.MobileTabsFor(hasCustomer: false).ShouldBe([GxNavigation.Administration]);
        GxNavigation.MobileTabsFor(hasCustomer: true).ShouldBe(GxNavigation.MobileTabs);
    }
}
