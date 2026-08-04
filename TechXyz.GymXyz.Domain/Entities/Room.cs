using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public class Room : EntityBase<int>, ITenantScoped
{
    public Room(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }
}