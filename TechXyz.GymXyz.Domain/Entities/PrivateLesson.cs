namespace TechXyz.GymXyz.Domain.Entities;

public sealed class PrivateLesson : Lesson
{
    public Location Location { get; set; }
    public Member? Member { get; set; }
}