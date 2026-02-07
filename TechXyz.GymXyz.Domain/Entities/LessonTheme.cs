using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public class LessonTheme : EntityBase<int>
{
    public LessonTheme(string name)
    {
        Name = name;
    }
    
    public string Name { get; set; }
    public string? Description { get; set; }
}