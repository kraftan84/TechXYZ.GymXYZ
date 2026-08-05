using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

public sealed record LessonDetailsDto(
    int Id,
    string Name,
    string? Description,
    LessonType Type,
    int? ThemeId,
    string? ThemeName,
    int CoachId,
    string CoachFirstName,
    string CoachLastName,
    DateTime StartDate,
    DateTime EndDate,
    List<LocationOptionDto> Locations,
    int? MaxParticipants);
