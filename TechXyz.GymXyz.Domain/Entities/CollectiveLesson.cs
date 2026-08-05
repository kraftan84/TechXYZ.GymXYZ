namespace TechXyz.GymXyz.Domain.Entities;

public sealed class CollectiveLesson : Lesson
{
    public int MaxParticipants { get; set; }
    public ICollection<Location> Locations { get; set; }
    public ICollection<Member>? Participants { get; set; }
    
    // Convenience count for UI and reporting when Participants is not loaded.
    public int NumberOfParticipants => Participants?.Count ?? 0;
}
