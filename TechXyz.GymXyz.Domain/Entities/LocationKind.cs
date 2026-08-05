namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// The three natures of venue the catalogue mixes in one list. The kind decides
/// what the record can say about a venue: a studio has a floor and a surface, an
/// outdoor spot has a meeting point and weather to worry about, and a session at
/// home happens at an address the venue does not hold.
/// </summary>
public enum LocationKind
{
    Studio,
    Outdoor,
    Home
}
