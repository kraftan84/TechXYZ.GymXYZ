using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What a coach teaches and, from lot 3 on, what a course belongs to. A short
/// referential: the mockup has no screen to manage it, so the set is seeded.
/// </summary>
public class Discipline : EntityBase<int>, ITenantScoped
{
    public Discipline(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }

    /// <summary>Lucide icon key, used by the course tiles of lot 3.</summary>
    public string? IconKey { get; set; }

    /// <summary>Tile tone — "brand", "success", "warning", "danger".</summary>
    public string? Tone { get; set; }

    public ICollection<CoachDiscipline>? Coaches { get; set; }
}
