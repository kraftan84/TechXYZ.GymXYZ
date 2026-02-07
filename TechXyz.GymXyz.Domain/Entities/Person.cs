using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public abstract class Person : EntityBase<int>
{
    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Address? Address { get; set; }
}