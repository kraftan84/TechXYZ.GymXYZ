using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public abstract class Lesson : EntityBase<int>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public LessonTheme? Theme { get; set; }
    public Coach Coach { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}