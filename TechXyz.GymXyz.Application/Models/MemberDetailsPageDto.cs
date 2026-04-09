using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

public sealed record MemberDetailsPageDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    AddressDto? Address,
    MemberStatsDto Stats,
    List<MemberSubscriptionDto> Subscriptions,
    List<MemberLessonDto> Lessons);

public sealed record MemberStatsDto(
    int TotalLessons,
    int AttendanceRate,
    DateOnly? LastVisit,
    int SubscriptionRemainingPercent);

public sealed record MemberSubscriptionDto(
    int Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int LessonsTotal,
    int LessonsRemaining,
    MemberSubscriptionStatus Status);

public enum MemberSubscriptionStatus
{
    Active,
    Paused,
    Expired
}

public sealed record MemberLessonDto(
    int Id,
    string Name,
    LessonType Type,
    DateTime StartDate,
    DateTime EndDate,
    string CoachFirstName,
    string CoachLastName,
    MemberLessonStatus Status,
    int Capacity,
    int RemainingSpots);

public enum MemberLessonStatus
{
    Pending,
    Confirmed,
    Completed
}
