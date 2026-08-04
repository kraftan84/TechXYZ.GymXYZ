namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Two breakpoints of the same product, not two products. A page renders one
/// layout or the other from a single query view model — never duplicated logic.
/// </summary>
public enum DeviceMode
{
    /// <summary>≥ 900px: sidebar 256px + topbar 64px, grids degraded below 1080px.</summary>
    Desktop,

    /// <summary>&lt; 900px: sticky header, scrollable body, 5-tab bar.</summary>
    Mobile
}
