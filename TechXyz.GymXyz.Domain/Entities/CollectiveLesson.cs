namespace TechXyz.GymXyz.Domain.Entities;

public sealed class CollectiveLesson : Lesson
{
    public int MaxParticipants { get; set; }
    public ICollection<Room> Rooms { get; set; }
    public ICollection<Member>? Participants { get; set; }
    
    public int NumberOfParticipants => Participants?.Count ?? 0;
}