using Shouldly;
using TechXyz.GymXyz.WebApp.Components.Layout;

namespace TechXYZ.GymXYZ.WebApp.Tests.Layout;

public class GxNavigationTests
{
    /// <summary>Somebody who runs a gym: the viewer every section was drawn for.</summary>
    private static readonly GxNavViewer Manager = new();

    /// <summary>A salaried coach — the role this lot finally gives a perimeter.</summary>
    private static readonly GxNavViewer Coach = new(IsManager: false);

    /// <summary>A platform admin who has entered no customer.</summary>
    private static readonly GxNavViewer Admin =
        new(HasCustomer: false, IsPlatformAdmin: true);

    [Fact]
    public void Visible_ShouldKeepCoachs_ForARegularGym()
    {
        var items = GxNavigation.Visible(GxNavigation.Groups[1].Items, Manager).ToList();

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
        var items = GxNavigation.Visible(GxNavigation.Groups[1].Items, Manager with { IsSolo = true }).ToList();

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
        var items = GxNavigation.Visible(GxNavigation.MobileMore, Manager with { IsSolo = true }).ToList();

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

        var items = GxNavigation.Visible(everything, Admin).ToList();

        items.ShouldBe([GxNavigation.Administration]);
    }

    [Fact]
    public void Visible_ShouldDropReglages_WithoutACustomer()
    {
        // Réglages is the customer's own settings — its identity, its team, its
        // e-mail. Named on its own because it sits in the footer rather than in
        // a group, and is the one business entry easy to forget.
        GxNavigation.Visible([GxNavigation.Reglages], Admin)
            .ShouldBeEmpty();
    }

    [Fact]
    public void Visible_ShouldLeaveAManagerEverySection()
    {
        // The half of this change that could quietly take access away from the
        // person who runs the gym. Asserted for the solo coach too, whose only
        // missing section is the one that was already hidden from them.
        foreach (var viewer in new[] { Manager, Manager with { IsSolo = true } })
        {
            var items = GxNavigation.Groups.SelectMany(group => group.Items).ToList();
            var visible = GxNavigation.Visible(items, viewer).ToList();

            visible.ShouldBe(items.Except(viewer.IsSolo ? [GxNavigation.Coachs] : []));
        }
    }

    [Fact]
    public void Visible_ShouldLeaveACoachOnlyWhatTheirScopeNames()
    {
        // TeamAccessScopes.Coach reads « Planning, cours & présences ». The
        // dashboard and the member list come with it — a coach lands somewhere
        // and points a sheet against a list of people.
        var everything = GxNavigation.Groups
            .SelectMany(group => group.Items)
            .Concat(GxNavigation.MobileMore)
            .Distinct();

        GxNavigation.Visible(everything, Coach).ShouldBe(
        [
            GxNavigation.Accueil,
            GxNavigation.Planning,
            GxNavigation.Presences,
            GxNavigation.Membres,
            GxNavigation.Cours
        ]);
    }

    [Theory]
    [InlineData("abos")]
    [InlineData("reglages")]
    [InlineData("coachs")]
    [InlineData("salles")]
    public void Visible_ShouldDropTheManagerSections_ForACoach(string id)
    {
        // Named one by one rather than only as a set: when somebody widens a
        // coach's reach later, the test that fails should say which section.
        var item = GxNavigation.Groups
            .SelectMany(group => group.Items)
            .Concat(GxNavigation.MobileMore)
            .First(candidate => candidate.Id == id);

        GxNavigation.Visible([item], Coach).ShouldBeEmpty();
        GxNavigation.Visible([item], Manager).ShouldBe([item]);
    }

    [Fact]
    public void Visible_ShouldOfferAdministration_ToNobodyButThePlatform()
    {
        // Rendered outside the filter until this lot, so every coach was shown a
        // link that answers "accès refusé".
        GxNavigation.Visible([GxNavigation.Administration], Coach).ShouldBeEmpty();
        GxNavigation.Visible([GxNavigation.Administration], Manager).ShouldBeEmpty();
        GxNavigation.Visible([GxNavigation.Administration], Admin)
            .ShouldBe([GxNavigation.Administration]);
    }

    [Fact]
    public void Visible_ShouldTreatAPlatformAdminInsideACustomerAsItsManager()
    {
        // GymPolicies.GymManager admits a PlatformAdmin, so hiding the sections
        // they are about to be allowed to open would only look broken.
        var inside = new GxNavViewer(IsPlatformAdmin: true);
        var items = GxNavigation.Groups.SelectMany(group => group.Items).ToList();

        GxNavigation.Visible(items, inside).ShouldBe(GxNavigation.Visible(items, Manager));
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
