namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Carries the navigation counters to the two shells: sheets to point on the
/// Présences item, covers needing action on the Abonnements one.
/// <para>
/// The counters come from the screens that already ran the query — the Accueil
/// on first load, then Présences and Abonnements as they are visited — rather
/// than the layout running its own. That is what makes a badge appear before
/// anybody has been to the screen it points at, which it did not do while this
/// only ever heard from Présences.
/// </para>
/// <para>
/// They cannot live on <c>GxNavItem</c>: that is a static record describing the
/// navigation, and these are figures that change as sheets get validated and
/// covers get renewed. Scoped, so they follow the circuit — same shape as
/// <see cref="MobileHeaderService"/>.
/// </para>
/// </summary>
public sealed class NavBadgeService
{
    /// <summary>Sheets waiting to be pointed. Null until something has answered.</summary>
    public int? SheetsToPoint { get; private set; }

    /// <summary>Covers expiring or late — what somebody has to act on.</summary>
    public int? SubscriptionsToWatch { get; private set; }

    public event Action? Changed;

    public void SetSheetsToPoint(int count)
    {
        if (Assign(SheetsToPoint, count, out var value))
        {
            SheetsToPoint = value;
            Changed?.Invoke();
        }
    }

    public void SetSubscriptionsToWatch(int count)
    {
        if (Assign(SubscriptionsToWatch, count, out var value))
        {
            SubscriptionsToWatch = value;
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// A count of nought is no badge, not a badge reading "0": a shell drawing
    /// zero would be reporting work where there is none.
    /// </summary>
    private static bool Assign(int? current, int count, out int? value)
    {
        value = count > 0 ? count : null;

        return current != value;
    }
}
