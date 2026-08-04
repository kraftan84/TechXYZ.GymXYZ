namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// How hard a course template is. Also what tints the discipline tile, which is
/// why a one-to-one coach lesson carries its own value rather than a level.
/// </summary>
public enum CourseIntensity
{
    Gentle,
    Moderate,
    High,
    Private
}
