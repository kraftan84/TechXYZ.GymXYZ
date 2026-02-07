using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public class Room : EntityBase<int>
{
    public Room(string name)
    {
        Name = name;
    }
    
    public string Name { get; set; }
}