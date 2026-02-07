using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Common;

public class EntityBase<T> : AuditableEntityBase, IEntity<T>
{
    public T Id { get; set; }
}