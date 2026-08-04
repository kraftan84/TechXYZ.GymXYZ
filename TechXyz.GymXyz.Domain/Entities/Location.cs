using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public class Location : EntityBase<int>, ITenantScoped
{
    public Location(string name)
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