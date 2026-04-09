using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonCommand : IRequest<int>
{
    public CreateLessonCommand(
        string name,
        string? description,
        LessonType type,
        int? themeId,
        int coachId,
        DateTime startDate,
        DateTime endDate,
        int roomId,
        int? maxParticipants)
    {
        Name = name;
        Description = description;
        Type = type;
        ThemeId = themeId;
        CoachId = coachId;
        StartDate = startDate;
        EndDate = endDate;
        RoomId = roomId;
        MaxParticipants = maxParticipants;
    }

    public string Name { get; }
    public string? Description { get; }
    public LessonType Type { get; }
    public int? ThemeId { get; }
    public int CoachId { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public int RoomId { get; }
    public int? MaxParticipants { get; }
}
