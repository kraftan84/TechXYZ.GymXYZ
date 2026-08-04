using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Layout;

public sealed record GxNavItem(
    string Id,
    string Label,
    string Icon,
    string Href,
    bool HiddenWhenSolo = false);

public sealed record GxNavGroup(string Title, IReadOnlyList<GxNavItem> Items);

/// <summary>
/// Single source of truth for both shells. Existing routes are kept; only the
/// labels follow the prototype.
/// </summary>
public static class GxNavigation
{
    public static readonly GxNavItem Accueil = new("accueil", "Accueil", GxIconPaths.Home, "/");
    public static readonly GxNavItem Planning = new("planning", "Planning", GxIconPaths.Calendar, "/plannings");
    public static readonly GxNavItem Presences = new("presences", "Présences", GxIconPaths.Check, "/presences");
    public static readonly GxNavItem Membres = new("membres", "Membres", GxIconPaths.Users, "/members");

    /// <summary>Hidden for a solo coach: the section does not exist for them.</summary>
    public static readonly GxNavItem Coachs = new("coachs", "Coachs", GxIconPaths.User, "/coachs", HiddenWhenSolo: true);

    public static readonly GxNavItem Cours = new("cours", "Cours", GxIconPaths.Dumbbell, "/cours");
    public static readonly GxNavItem Abonnements = new("abos", "Abonnements", GxIconPaths.Card, "/abonnements");
    public static readonly GxNavItem Lieux = new("salles", "Lieux", GxIconPaths.Pin, "/rooms");
    public static readonly GxNavItem Reglages = new("reglages", "Réglages", GxIconPaths.Settings, "/reglages");
    public static readonly GxNavItem Administration = new("administration", "Administration", GxIconPaths.Shield, "/administration");

    public static readonly IReadOnlyList<GxNavGroup> Groups =
    [
        new("Pilotage", [Accueil, Planning, Presences]),
        new("Personnes", [Membres, Coachs]),
        new("Offre & business", [Cours, Abonnements]),
        new("Lieux", [Lieux])
    ];

    /// <summary>Bottom tab bar. The fifth tab opens the "Plus" sheet.</summary>
    public static readonly IReadOnlyList<GxNavItem> MobileTabs = [Accueil, Planning, Presences, Membres];

    /// <summary>The rest of the navigation, shown in the "Plus" sheet.</summary>
    public static readonly IReadOnlyList<GxNavItem> MobileMore =
        [Coachs, Cours, Abonnements, Lieux, Reglages, Administration];

    public static IEnumerable<GxNavItem> Visible(IEnumerable<GxNavItem> items, bool isSolo)
        => items.Where(item => !(item.HiddenWhenSolo && isSolo));
}
