using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Layout;

public sealed record GxNavItem(
    string Id,
    string Label,
    string Icon,
    string Href,
    bool HiddenWhenSolo = false,
    bool NeedsCustomer = true,
    bool RequiresManager = false,
    bool RequiresPlatformAdmin = false)
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
/// Who is looking, in the only four terms the navigation cares about. A record
/// rather than four positional booleans, because <c>Visible(items, true, false,
/// true, false)</c> is unreadable at the call site and silently wrong when two
/// of them are swapped.
/// <para>
/// <paramref name="IsManager"/> is true for a platform admin too, matching
/// <c>GymPolicies.GymManager</c>: an admin inside a customer stands in for its
/// manager, and hiding the sections they are about to be allowed to open would
/// only make the console look broken.
/// </para>
/// </summary>
public sealed record GxNavViewer(
    bool IsSolo = false,
    bool HasCustomer = true,
    bool IsManager = true,
    bool IsPlatformAdmin = false);

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

    /// <summary>
    /// Hidden for a solo coach: the section does not exist for them. Manager-only
    /// besides — a coach reads their colleagues' names off the planning, which is
    /// the part of them that is their business; the directory holds e-mail
    /// addresses and phone numbers, which is not.
    /// </summary>
    public static readonly GxNavItem Coachs =
        new("coachs", "Coachs", GxIconPaths.User, "/coachs", HiddenWhenSolo: true, RequiresManager: true);

    public static readonly GxNavItem Cours = new("cours", "Cours", GxIconPaths.Dumbbell, "/cours");

    public static readonly GxNavItem Abonnements =
        new("abos", "Abonnements", GxIconPaths.Card, "/abonnements", RequiresManager: true)
        { ShortLabel = "Abos" };

    public static readonly GxNavItem Lieux =
        new("salles", "Lieux", GxIconPaths.Pin, "/lieux", RequiresManager: true);

    public static readonly GxNavItem Reglages =
        new("reglages", "Réglages", GxIconPaths.Settings, "/reglages", RequiresManager: true);

    /// <summary>
    /// The one section that belongs to the platform rather than to a customer,
    /// and therefore the only one still reachable when no customer is chosen.
    /// </summary>
    public static readonly GxNavItem Administration =
        new("administration", "Administration", GxIconPaths.Shield, "/administration",
            NeedsCustomer: false, RequiresPlatformAdmin: true)
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

    /// <summary>
    /// What this viewer may see. A solo coach has no Coachs section; a platform
    /// admin who has entered no customer has no business sections at all, because
    /// there is no customer for them to be about — every one of those screens
    /// would query the ambient tenant and come back empty; and a salaried coach
    /// has no section that is about running the gym rather than teaching in it.
    /// <para>
    /// Hiding them is the readable half of the fix, not the enforcing half: the
    /// URL still has to answer for itself, which is why the resolver refuses the
    /// host fallback and why every hidden screen also carries a policy attribute.
    /// </para>
    /// </summary>
    public static IEnumerable<GxNavItem> Visible(IEnumerable<GxNavItem> items, GxNavViewer viewer)
        => items.Where(item =>
            !(item.HiddenWhenSolo && viewer.IsSolo)
            && (viewer.HasCustomer || !item.NeedsCustomer)
            && (viewer.IsManager || !item.RequiresManager)
            && (viewer.IsPlatformAdmin || !item.RequiresPlatformAdmin));
}
