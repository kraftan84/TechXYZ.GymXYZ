using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public class LessonTheme : EntityBase<int>, ITenantScoped
{
    public LessonTheme(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }
    public string? Description { get; set; }
}