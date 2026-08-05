namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One option of the site picker: the buildings a studio can sit in. Sites have
/// no screen of their own — the pre-hand-off one was dropped with lot 4 and the
/// hand-off designs none — so this is the only place they surface.
/// </summary>
public sealed record SiteOptionDto(
    int Id,
    string Name);
