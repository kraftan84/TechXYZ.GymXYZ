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
    List<MemberSessionDto> UpcomingSessions,
    List<MemberSessionDto> PastSessions,
    List<MemberPaymentDto> Payments,
    MemberStatsDto Stats)
{
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Numbers on the record. The rate is null while none of the member's seats has
/// been pointed — "—", never a nought that would read as a verdict.
/// </summary>
public sealed record MemberStatsDto(
    int TotalSessions,
    int? AttendanceRate,
    DateOnly? LastVisitOn);

/// <summary>
/// One line of the member's subscription history — what they bought, when, and
/// where it stands.
/// </summary>
public sealed record MemberSubscriptionDto(
    int Id,
    int PlanId,
    string PlanName,
    PlanKind Kind,
    DateOnly StartedOn,
    DateOnly EndsOn,
    int? CreditsRemaining,
    int? CreditsTotal,
    string PriceLabel,
    bool AutoRenew,
    SubscriptionStatus? Status)
{
    /// <summary>
    /// Null status means the cover has not begun: a renewal booked for after the
    /// current one runs out. It has no standing yet, and saying it were "Actif"
    /// would be a lie the history list tells about next month.
    /// </summary>
    public bool HasStarted => Status is not null;

    public string CreditsLabel => Kind == PlanKind.CreditPack
        ? $"{Math.Max(0, CreditsRemaining ?? 0)}/{CreditsTotal ?? 0}"
        : "∞";
}

/// <summary>One line of the member's payments card.</summary>
public sealed record MemberPaymentDto(
    int Id,
    DateOnly Date,
    string Label,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status);

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
    int RemainingSpots,
    AttendanceStatus AttendanceStatus)
{
    public bool IsPrivate => Capacity == 1;

    /// <summary>
    /// Whether the sheet recorded anything for this seat. "Présences récentes"
    /// chips a past session « Passé » only while nobody pointed it — once
    /// pointed it says présent, en retard or absent.
    /// </summary>
    public bool IsPointed => AttendanceStatus != AttendanceStatus.Pending;
}
