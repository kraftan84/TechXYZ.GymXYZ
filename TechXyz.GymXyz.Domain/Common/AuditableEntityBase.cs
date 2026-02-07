using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Common;

public abstract class AuditableEntityBase : IAuditableEntity
{
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}