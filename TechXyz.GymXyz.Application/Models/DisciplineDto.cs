namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One entry of the discipline referential, as offered by the coach drawer.
/// No screen manages the list yet: the set is seeded and picked from.
/// </summary>
public sealed record DisciplineDto(
    int Id,
    string Name,
    string? IconKey,
    string? Tone);
