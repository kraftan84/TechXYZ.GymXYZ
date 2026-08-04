using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A discipline a coach teaches. An explicit join because the order matters:
/// the first pill of the card is the one tinted in the brand colour.
/// </summary>
public class CoachDiscipline : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int CoachId { get; set; }
    public Coach? Coach { get; set; }

    public int DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }

    public int Rank { get; set; }
}
