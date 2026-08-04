using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Features.Members;

/// <summary>
/// The four filter chips, and the wording each standing carries. Desktop and
/// mobile label the warning chip differently — both come from the prototype and
/// are kept word for word.
/// </summary>
public sealed record MemberFilter(MemberStatus? Status, string DesktopLabel, string MobileLabel);

public static class MemberFilters
{
    public static readonly IReadOnlyList<MemberFilter> All =
    [
        new(null, "Tous", "Tous"),
        new(MemberStatus.Active, "Actifs", "Actifs"),
        new(MemberStatus.ExpiringSoon, "Expire bientôt", "À relancer"),
        new(MemberStatus.Inactive, "Inactifs", "Inactifs")
    ];

    public static string LabelFor(MemberStatus status) => status switch
    {
        MemberStatus.Active => "Actif",
        MemberStatus.ExpiringSoon => "Expire bientôt",
        _ => "Inactif"
    };

    /// <summary>Status tones carry meaning and are never themed.</summary>
    public static GxTone ToneFor(MemberStatus status) => status switch
    {
        MemberStatus.Active => GxTone.Success,
        MemberStatus.ExpiringSoon => GxTone.Warning,
        _ => GxTone.Danger
    };
}
