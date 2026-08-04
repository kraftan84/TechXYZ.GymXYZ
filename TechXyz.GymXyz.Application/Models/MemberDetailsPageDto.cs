using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

public sealed record MemberDetailsPageDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly JoinedOn,
    DateOnly? BirthDate,
    string? Notes,
    AddressDto? Address,
    MemberStatus Status,
    MemberSubscriptionDto? CurrentSubscription,
    List<MemberSubscriptionDto> Subscriptions,
    List<MemberLessonDto> UpcomingLessons,
    List<MemberLessonDto> PastLessons,
    List<MemberPaymentDto> Payments,
    MemberStatsDto Stats)
{
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Numbers on the record. The two nullable ones are produced by attendance
/// check-in and stay unset until lot 6 — they are shown as "—", never guessed.
/// </summary>
public sealed record MemberStatsDto(
    int TotalLessons,
    int? AttendanceRate,
    DateOnly? LastVisitOn);

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

/// <summary>Placeholder shape for the payments card. Filled at lot 7.</summary>
public sealed record MemberPaymentDto(
    DateOnly Date,
    string Label,
    decimal Amount,
    string Status);

public sealed record MemberLessonDto(
    int Id,
    string Name,
    LessonType Type,
    DateTime StartDate,
    DateTime EndDate,
    string CoachFirstName,
    string CoachLastName,
    int Capacity,
    int RemainingSpots);
