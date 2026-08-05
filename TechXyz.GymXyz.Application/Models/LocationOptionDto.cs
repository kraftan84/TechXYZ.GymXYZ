namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One option of a venue picker: the least a drawer needs to offer a choice.
/// The catalogue of venues itself is a different, much wider projection.
/// </summary>
public sealed record LocationOptionDto(
    int Id,
    string Name);
