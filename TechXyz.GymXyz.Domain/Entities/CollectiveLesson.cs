namespace TechXyz.GymXyz.Domain.Entities;

public sealed class CollectiveLesson : Lesson
{
    public ICollection<Room> Rooms { get; set; }
    public ICollection<Member>? Members { get; set; }
}