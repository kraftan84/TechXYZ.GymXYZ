namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Who a course template is written for. Displayed as a chip on the record; the
/// French wordings live in the Application layer.
/// </summary>
public enum CourseLevel
{
    AllLevels,
    Beginner,
    Intermediate,
    Custom
}
