namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Holds the layout mode for the current circuit. Fed by a matchMedia listener,
/// never by user-agent sniffing.
/// </summary>
public sealed class ResponsiveModeService
{
    public const string CookieName = "gx-device";

    /// <summary>Mobile shell below this width, desktop shell at or above it.</summary>
    public const int MobileBreakpointPx = 900;

    public DeviceMode Mode { get; private set; } = DeviceMode.Desktop;

    public bool IsMobile => Mode == DeviceMode.Mobile;

    public event Action? Changed;

    /// <summary>
    /// Seeds the mode from the cookie written on the previous visit, so the first
    /// server render already picks the right shell instead of flashing the other one.
    /// </summary>
    public void SeedFromCookie(string? cookieValue)
    {
        Mode = string.Equals(cookieValue, "mobile", StringComparison.OrdinalIgnoreCase)
            ? DeviceMode.Mobile
            : DeviceMode.Desktop;
    }

    public void SetMode(DeviceMode mode)
    {
        if (Mode == mode)
            return;

        Mode = mode;
        Changed?.Invoke();
    }
}
