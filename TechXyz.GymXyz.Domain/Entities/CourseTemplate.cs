using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A course as the catalogue defines it, not as it happens: "Power Cycle, 45
/// minutes, 24 places, Studio C". The dated occurrence is a different thing and
/// arrives at lot 5 under the name Session.
/// </summary>
public class CourseTemplate : EntityBase<int>, ITenantScoped
{
    public CourseTemplate(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }

    public int DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }

    /// <summary>
    /// Lucide icon key, only when the course wants a different one from its
    /// discipline. Null is the normal case and falls back to the discipline.
    /// </summary>
    public string? IconKey { get; set; }

    public int DurationMinutes { get; set; }

    /// <summary>
    /// How many people fit. A capacity of one is what makes a course private —
    /// there is no separate type to keep in step with it.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>Studio the planning proposes first. Optional.</summary>
    public int? DefaultLocationId { get; set; }
    public Location? DefaultLocation { get; set; }

    public CourseLevel Level { get; set; }

    public CourseIntensity Intensity { get; set; }

    /// <summary>
    /// Price per session. Null means the course is included in the subscription,
    /// which is what the catalogue shows as "Inclus".
    /// </summary>
    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public ICollection<CourseTemplateCoach>? Coaches { get; set; }

    public void AddCoach(Coach coach, int rank)
    {
        Coaches ??= new List<CourseTemplateCoach>();
        Coaches.Add(new CourseTemplateCoach { Coach = coach, Rank = rank });
    }
}
