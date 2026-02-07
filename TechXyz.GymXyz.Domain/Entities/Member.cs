namespace TechXyz.GymXyz.Domain.Entities;

public class Member : Person
{
    public Member(string firstName, string lastName) 
        : base(firstName, lastName)
    {
    }

    public ICollection<Subscription>? Subscriptions { get; set; }
    public ICollection<PrivateLesson>? PrivateLessons { get; set; }
    public ICollection<CollectiveLesson>? CollectiveLessons { get; set; }
}