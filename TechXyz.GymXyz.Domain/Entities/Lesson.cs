using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public abstract class Lesson : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public LessonTheme? Theme { get; set; }
    public Coach Coach { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}