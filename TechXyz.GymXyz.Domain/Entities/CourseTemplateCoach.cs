using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A coach allowed to run a course template. An explicit join because the order
/// matters: it is the order the avatars stack in on the catalogue row.
/// </summary>
public class CourseTemplateCoach : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int CourseTemplateId { get; set; }
    public CourseTemplate? CourseTemplate { get; set; }

    public int CoachId { get; set; }
    public Coach? Coach { get; set; }

    public int Rank { get; set; }
}
