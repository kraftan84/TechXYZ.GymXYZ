using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A postal address the gym operates from — the building itself, not one of the
/// venues inside it. Named <c>Location</c> until lot 4, where the data model gave
/// that name to the venue a session is booked in.
/// </summary>
public class Site : EntityBase<int>, ITenantScoped
{
    public Site(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }
    public Address Address { get; set; }
    public ICollection<Location>? Locations { get; set; }

    public void AddLocation(Location location)
    {
        Locations ??= new List<Location>();
        Locations.Add(location);
    }
}