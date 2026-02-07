using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public class Location : EntityBase<int>
{
    public Location(string name)
    {
        Name = name;
    }
    
    public string Name { get; set; }
    public Address Address { get; set; }
    public ICollection<Room>? Rooms { get; set; }

    public void AddRoom(Room room)
    {
        Rooms ??= new List<Room>();
        Rooms.Add(room);
    }
}