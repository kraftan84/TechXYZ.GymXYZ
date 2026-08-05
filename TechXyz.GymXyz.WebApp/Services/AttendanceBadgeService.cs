namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Carries the "à pointer" count to the two shells, which draw it on the
/// Présences tab. <c>04-SCREENS-MOBILE.md</c> asks for the counter and says it
/// comes from the same query as the KPI, so it is published by the screen that
/// already ran that query rather than run a second time by the layout.
/// <para>
/// It cannot live on <c>GxNavItem</c>: that is a static record describing the
/// navigation, and this is a figure that changes as sheets get validated.
/// Scoped, so it follows the circuit — same shape as
/// <see cref="MobileHeaderService"/>.
/// </para>
/// </summary>
public sealed class AttendanceBadgeService
{
    /// <summary>Null until the Présences screen has been loaded once — no badge, rather than a zero.</summary>
    public int? SheetsToPoint { get; private set; }

    public event Action? Changed;

    public void Set(int count)
    {
        var value = count > 0 ? count : (int?)null;

        if (SheetsToPoint == value)
        {
            return;
        }

        SheetsToPoint = value;
        Changed?.Invoke();
    }
}
