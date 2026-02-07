namespace TechXyz.GymXyz.Domain.Entities;

public class Coach : Person
{
    public Coach(string firstName, string lastName) 
        : base(firstName, lastName)
    {
    }

    public ICollection<PrivateLesson>? PrivateLessons { get; set; }
    public ICollection<CollectiveLesson>? CollectiveLessons { get; set; }
}