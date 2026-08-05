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
    List<MemberSessionDto> UpcomingSessions,
    List<MemberSessionDto> PastSessions,
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
    int TotalSessions,
    int? AttendanceRate,
    DateOnly? LastVisitOn);

public sealed record MemberSubscriptionDto(
    int Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int SessionsTotal,
    int SessionsRemaining,
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

/// <summary>
/// One session the member has a seat on. There is no type to carry: a capacity
/// of one is what makes it private, the same rule the catalogue and the planning
/// use.
/// </summary>
public sealed record MemberSessionDto(
    int Id,
    string Name,
    DateTime StartsAt,
    DateTime EndsAt,
    string? CoachFirstName,
    string? CoachLastName,
    int Capacity,
    int RemainingSpots)
{
    public bool IsPrivate => Capacity == 1;
}
