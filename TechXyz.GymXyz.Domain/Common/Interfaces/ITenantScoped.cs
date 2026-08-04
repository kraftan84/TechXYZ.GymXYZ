namespace TechXyz.GymXyz.Domain.Common.Interfaces;

/// <summary>
/// Marks an entity as belonging to a single tenant (a GymXYZ customer).
/// The tenant filter is applied globally by <c>GymDbContext</c>; soft delete
/// (<c>IsActive</c>) stays an explicit per-query concern.
/// </summary>
public interface ITenantScoped
{
    int TenantId { get; set; }
}
