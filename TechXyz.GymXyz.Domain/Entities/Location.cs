using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A venue a session is booked in — a studio today, and at lot 4 also an
/// outdoor spot or the member's own home. Called <c>Room</c> until then, which
/// none of the three kinds beyond the first is.
/// </summary>
public class Location : EntityBase<int>, ITenantScoped
{
    public Location(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }
}