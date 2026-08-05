using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A postal address the gym operates from — the building, not a room inside it.
/// Named <c>Location</c> until lot 4, where the data model gave that name to the
/// venue a session is booked in.
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
    public ICollection<Room>? Rooms { get; set; }

    public void AddRoom(Room room)
    {
        Rooms ??= new List<Room>();
        Rooms.Add(room);
    }
}