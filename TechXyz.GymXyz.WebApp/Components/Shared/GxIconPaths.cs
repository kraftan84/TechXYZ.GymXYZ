namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// Lucide-style line icons, outline only, ~1.8px on a 24 grid — the TechXYZ
/// iconography rules. Ported from the prototype so the app carries no icon
/// package, and so Lucide and Fluent icons never mix inside one view.
/// </summary>
public static class GxIconPaths
{
    public const string Home = "home";
    public const string Calendar = "calendar";
    public const string Check = "check";
    public const string Users = "users";
    public const string User = "user";
    public const string Dumbbell = "dumbbell";
    public const string Card = "card";
    public const string Pin = "pin";
    public const string Shield = "shield";
    public const string Settings = "settings";
    public const string Search = "search";
    public const string Bell = "bell";
    public const string Grid = "grid";
    public const string Sparkles = "sparkles";
    public const string ArrowRight = "arrowR";
    public const string ChevronRight = "chevR";
    public const string ChevronLeft = "chevL";
    public const string Close = "x";
    public const string Plus = "plus";
    public const string Filter = "filter";
    public const string Mail = "mail";
    public const string Phone = "phone";
    public const string Trash = "trash";
    public const string History = "history";
    public const string Clock = "clock";
    public const string Minus = "minus";
    public const string Percent = "percent";
    public const string UserCheck = "userCheck";
    public const string Alert = "alert";
    public const string Euro = "euro";
    public const string Copy = "copy";
    public const string Target = "target";
    public const string Trend = "trend";
    public const string Tree = "tree";
    public const string Maximize = "maximize";
    public const string Building = "building";
    public const string Send = "send";
    public const string Cloud = "cloud";
    public const string Sun = "sun";
    public const string Star = "star";
    public const string Share = "share";

    private static readonly Dictionary<string, string> Paths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["home"] = """<path d="M3 11l9-8 9 8"/><path d="M5 10v10h14V10"/>""",
        ["calendar"] = """<rect x="3" y="4" width="18" height="17" rx="2"/><path d="M3 9h18M8 2v4M16 2v4"/>""",
        ["user"] = """<circle cx="12" cy="8" r="4"/><path d="M4 21c0-4 4-6 8-6s8 2 8 6"/>""",
        ["users"] = """<circle cx="9" cy="8" r="3.4"/><path d="M2 21c0-3.5 3.2-5.5 7-5.5"/><circle cx="17" cy="9" r="2.8"/><path d="M14.5 16c3 .4 5.5 2 5.5 5"/>""",
        ["dumbbell"] = """<path d="M3 9v6M6 7v10M18 7v10M21 9v6M6 12h12"/>""",
        ["card"] = """<rect x="2.5" y="5" width="19" height="14" rx="2"/><path d="M2.5 9.5h19"/>""",
        ["building"] = """<rect x="4" y="3" width="16" height="18" rx="1.5"/><path d="M9 7h2M13 7h2M9 11h2M13 11h2M9 15h2M13 15h2"/>""",
        ["settings"] = """<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/>""",
        ["bell"] = """<path d="M18 9a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7"/><path d="M10 20a2 2 0 0 0 4 0"/>""",
        ["search"] = """<circle cx="11" cy="11" r="7"/><path d="m20 20-3.2-3.2"/>""",
        ["plus"] = """<path d="M12 5v14M5 12h14"/>""",
        ["check"] = """<path d="M5 12.5 10 17 19 7"/>""",
        ["x"] = """<path d="M6 6l12 12M18 6 6 18"/>""",
        ["clock"] = """<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>""",
        ["chevR"] = """<path d="m9 6 6 6-6 6"/>""",
        ["chevL"] = """<path d="m15 6-6 6 6 6"/>""",
        ["chevD"] = """<path d="m6 9 6 6 6-6"/>""",
        ["arrowR"] = """<path d="M5 12h14M13 6l6 6-6 6"/>""",
        ["send"] = """<path d="M22 3 11 14M22 3l-7 18-4-7-7-4 18-7z"/>""",
        ["share"] = """<circle cx="18" cy="5" r="3"/><circle cx="6" cy="12" r="3"/><circle cx="18" cy="19" r="3"/><path d="M8.6 13.5l6.8 4M15.4 6.5l-6.8 4"/>""",
        ["phone"] = """<path d="M5 3h4l2 5-3 2a12 12 0 0 0 5 5l2-3 5 2v4a2 2 0 0 1-2 2A17 17 0 0 1 3 5a2 2 0 0 1 2-2z"/>""",
        ["mail"] = """<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/>""",
        ["trend"] = """<path d="M3 17 9 11l4 4 8-8"/><path d="M21 7v5h-5"/>""",
        ["alert"] = """<path d="M12 3 2 20h20L12 3z"/><path d="M12 10v4M12 17h.01"/>""",
        ["grid"] = """<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>""",
        ["filter"] = """<path d="M3 5h18l-7 8v6l-4 2v-8L3 5z"/>""",
        ["pin"] = """<path d="M12 21s7-6.3 7-12a7 7 0 0 0-14 0c0 5.7 7 12 7 12z"/><circle cx="12" cy="9" r="2.5"/>""",
        ["eye"] = """<path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>""",
        ["download"] = """<path d="M12 3v12"/><path d="m7 11 5 5 5-5"/><path d="M4 21h16"/>""",
        ["refresh"] = """<path d="M21 12a9 9 0 1 1-2.6-6.4"/><path d="M21 3v6h-6"/>""",
        ["history"] = """<path d="M3 12a9 9 0 1 0 3-6.7L3 8"/><path d="M3 4v4h4"/><path d="M12 8v4l3 2"/>""",
        ["trash"] = """<path d="M4 7h16M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2m-8 0 1 13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1l1-13"/>""",
        ["palette"] = """<circle cx="13.5" cy="6.5" r="1.5"/><circle cx="17.5" cy="10.5" r="1.5"/><circle cx="8.5" cy="7.5" r="1.5"/><circle cx="6.5" cy="12.5" r="1.5"/><path d="M12 2a10 10 0 1 0 0 20 2.5 2.5 0 0 0 2-4 2.5 2.5 0 0 1 2-4h2a4 4 0 0 0 4-4 10 10 0 0 0-12-8"/>""",
        ["target"] = """<circle cx="12" cy="12" r="8"/><circle cx="12" cy="12" r="3.5"/>""",
        ["sparkles"] = """<path d="M12 3l1.6 4.4L18 9l-4.4 1.6L12 15l-1.6-4.4L6 9l4.4-1.6z"/><path d="M19 14l.8 2.2L22 17l-2.2.8L19 20l-.8-2.2L16 17l2.2-.8z"/>""",
        ["copy"] = """<rect x="9" y="9" width="11" height="11" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h8"/>""",
        ["euro"] = """<path d="M17 6.3A6 6 0 1 0 17 18M5 10h8M5 14h7"/>""",
        ["percent"] = """<path d="M19 5 5 19"/><circle cx="7.5" cy="7.5" r="2.5"/><circle cx="16.5" cy="16.5" r="2.5"/>""",
        ["minus"] = """<path d="M5 12h14"/>""",
        ["zap"] = """<path d="M13 2 4 14h7l-1 8 9-12h-7l1-8z"/>""",
        ["wallet"] = """<path d="M3 7a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v1H5a2 2 0 0 0-2 2zm0 4a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><circle cx="16.5" cy="14" r="1.2"/>""",
        ["userCheck"] = """<circle cx="9" cy="8" r="4"/><path d="M3 21c0-4 3.5-6 6-6s6 2 6 6"/><path d="m16 12 2 2 4-4"/>""",
        ["maximize"] = """<path d="M3 9V4a1 1 0 0 1 1-1h5M21 9V4a1 1 0 0 0-1-1h-5M3 15v5a1 1 0 0 0 1 1h5M21 15v5a1 1 0 0 1-1 1h-5"/>""",
        ["layers"] = """<path d="m12 3 9 5-9 5-9-5 9-5z"/><path d="m3 13 9 5 9-5"/>""",
        ["file"] = """<path d="M14 3v5h5"/><path d="M14 3H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>""",
        ["tree"] = """<path d="M12 2 5 12h4l-3 5h12l-3-5h4L12 2z"/><path d="M12 17v5"/>""",
        ["star"] = """<path d="M12 3l2.6 5.6 6.1.7-4.5 4.1 1.2 6-5.4-3-5.4 3 1.2-6L3.3 9.3l6.1-.7z"/>""",
        ["sun"] = """<circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M2 12h2M20 12h2M5 5l1.4 1.4M17.6 17.6 19 19M19 5l-1.4 1.4M6.4 17.6 5 19"/>""",
        ["cloud"] = """<path d="M7 18a4 4 0 0 1 .4-8A5.5 5.5 0 0 1 18 9.5 3.5 3.5 0 0 1 17.5 18z"/>""",
        ["shield"] = """<path d="M12 3l8 3v5c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6l8-3z"/><path d="m9 12 2 2 4-4"/>""",
        ["qr"] = """<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><path d="M14 14h3v3M20 14v.01M14 20h.01M20 20h.01M17 20v.01"/>"""
    };

    public static string Get(string name) => Paths.TryGetValue(name, out var path) ? path : string.Empty;
}
