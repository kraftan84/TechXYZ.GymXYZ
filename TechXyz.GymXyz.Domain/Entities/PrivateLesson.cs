namespace TechXyz.GymXyz.Domain.Entities;

public sealed class PrivateLesson : Lesson
{
    public Room Room { get; set; }
    public Member? Member { get; set; }
}