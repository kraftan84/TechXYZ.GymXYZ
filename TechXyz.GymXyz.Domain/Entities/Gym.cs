using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public class Gym : EntityBase<int>, ITenantScoped
{
    public Gym(string name)
    {
        Name = name;
    }

    public int TenantId { get; set; }

    public string Name { get; set; }

    public ICollection<Location>?  Locations { get; set; }
    
    public ICollection<Coach>? Coaches { get; set; }
    
    public ICollection<Member>? Members { get; set; }

    public void AddLocation(Location location)
    {
        Locations ??= new List<Location>();
        Locations.Add(location);
    }

    public void AddCoach(Coach coach)
    {
        Coaches ??= new List<Coach>();
        Coaches.Add(coach);
    }
    
    public void AddMember(Member member)
    {
        Members ??= new List<Member>();
        Members.Add(member);
    }
}