using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Layout;

public sealed record GxNavItem(
    string Id,
    string Label,
    string Icon,
    string Href,
    bool HiddenWhenSolo = false,
    bool NeedsCustomer = true)
{
    /// <summary>
    /// The label where the box is narrow — « Abos », « Admin. ». Defaults to the
    /// full one, so only the two that need shortening carry a second wording.
    /// <para>
    /// Stored rather than cut down from <see cref="Label"/> for the reason
    /// <c>Plan.ShortName</c> is: truncating on a word boundary would turn
    /// « Administration » into « Administra… », which is not a word anybody
    /// writes. Which of the two is shown is a question of available width, and
    /// the stylesheet answers it.
    /// </para>
    /// </summary>
    public string ShortLabel { get; init; } = Label;
}

public sealed record GxNavGroup(string Title, IReadOnlyList<GxNavItem> Items);

/// <summary>
/// Single source of truth for both shells. Existing routes are kept; only the
/// labels follow the prototype.
/// </summary>
public static class GxNavigation
{
    public static readonly GxNavItem Accueil = new("accueil", "Accueil", GxIconPaths.Home, "/");
    public static readonly GxNavItem Planning = new("planning", "Planning", GxIconPaths.Calendar, "/planning");
    public static readonly GxNavItem Presences = new("presences", "Présences", GxIconPaths.Check, "/presences");
    public static readonly GxNavItem Membres = new("membres", "Membres", GxIconPaths.Users, "/members");

    /// <summary>Hidden for a solo coach: the section does not exist for them.</summary>
    public static readonly GxNavItem Coachs = new("coachs", "Coachs", GxIconPaths.User, "/coachs", HiddenWhenSolo: true);

    public static readonly GxNavItem Cours = new("cours", "Cours", GxIconPaths.Dumbbell, "/cours");
    public static readonly GxNavItem Abonnements =
        new("abos", "Abonnements", GxIconPaths.Card, "/abonnements") { ShortLabel = "Abos" };

    public static readonly GxNavItem Lieux = new("salles", "Lieux", GxIconPaths.Pin, "/lieux");
    public static readonly GxNavItem Reglages = new("reglages", "Réglages", GxIconPaths.Settings, "/reglages");

    /// <summary>
    /// The one section that belongs to the platform rather than to a customer,
    /// and therefore the only one still reachable when no customer is chosen.
    /// </summary>
    public static readonly GxNavItem Administration =
        new("administration", "Administration", GxIconPaths.Shield, "/administration", NeedsCustomer: false)
        { ShortLabel = "Admin." };

    public static readonly IReadOnlyList<GxNavGroup> Groups =
    [
        new("Pilotage", [Accueil, Planning, Presences]),
        new("Personnes", [Membres, Coachs]),
        new("Offre & business", [Cours, Abonnements]),
        new("Lieux", [Lieux])
    ];

    /// <summary>Bottom tab bar. The fifth tab opens the "Plus" sheet.</summary>
    public static readonly IReadOnlyList<GxNavItem> MobileTabs = [Accueil, Planning, Presences, Membres];

    /// <summary>
    /// The tabs for this viewer. All four are about a customer, so an admin who
    /// has entered none gets Administration in their place rather than a bar
    /// holding nothing but "Plus" — which would read as a broken shell instead
    /// of a console.
    /// </summary>
    public static IReadOnlyList<GxNavItem> MobileTabsFor(bool hasCustomer) =>
        hasCustomer ? MobileTabs : [Administration];

    /// <summary>The rest of the navigation, shown in the "Plus" sheet.</summary>
    public static readonly IReadOnlyList<GxNavItem> MobileMore =
        [Coachs, Cours, Abonnements, Lieux, Reglages, Administration];

    public static IEnumerable<GxNavItem> Visible(IEnumerable<GxNavItem> items, bool isSolo)
        => Visible(items, isSolo, hasCustomer: true);

    /// <summary>
    /// What this viewer may see. A solo coach has no Coachs section; a platform
    /// admin who has entered no customer has no business sections at all, because
    /// there is no customer for them to be about — every one of those screens
    /// would query the ambient tenant and come back empty.
    /// <para>
    /// Hiding them is the readable half of the fix, not the enforcing half: the
    /// URL still has to answer for itself, which is why the resolver refuses the
    /// host fallback rather than relying on this.
    /// </para>
    /// </summary>
    public static IEnumerable<GxNavItem> Visible(
        IEnumerable<GxNavItem> items, bool isSolo, bool hasCustomer)
        => items.Where(item =>
            !(item.HiddenWhenSolo && isSolo)
            && (hasCustomer || !item.NeedsCustomer));
}
