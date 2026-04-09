using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

public sealed record LessonsPageDto(
    List<LessonListItemDto> Lessons,
    List<LessonThemeDto> Themes,
    List<LessonCoachDto> Coaches,
    List<RoomDto> Rooms);

public sealed record LessonListItemDto(
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
    List<RoomDto> Rooms,
    int? MaxParticipants);

public sealed record LessonThemeDto(
    int Id,
    string Name,
    string? Description);

public sealed record LessonCoachDto(
    int Id,
    string FirstName,
    string LastName);
