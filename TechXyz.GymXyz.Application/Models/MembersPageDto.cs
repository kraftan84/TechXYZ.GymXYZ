namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One page of the members list plus the counts behind the filter chips.
/// </summary>
public sealed record MembersPageDto(
    IReadOnlyList<MemberListItemDto> Items,
    int TotalCount,
    int ActiveCount,
    int ExpiringSoonCount,
    int InactiveCount,
    int FilteredCount)
{
    public static MembersPageDto Empty { get; } = new([], 0, 0, 0, 0, 0);
}

/// <summary>
/// A row of the members table. The columns whose source lands in a later lot are
/// nullable and left unset — they render as "—" rather than being invented.
/// </summary>
public sealed record MemberListItemDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly JoinedOn,
    IReadOnlyList<SubscriptionCoverDto> Covers)
{
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Set by the query handler from <c>MemberStatusRules</c>.</summary>
    public MemberStatus Status { get; init; }

    /// <summary>
    /// The cover the standing and the two credit columns are read from — the
    /// healthiest of <see cref="Covers"/>. Null for a member with none.
    /// </summary>
    public SubscriptionCoverDto? GoverningCover { get; init; }

    /// <summary>Subscription plan name, from <see cref="GoverningCover"/>.</summary>
    public string? PlanLabel { get; init; }

    /// <summary>Credits left, "3/10" or "∞".</summary>
    public string? CreditsLabel { get; init; }

    /// <summary>
    /// The gauge, 0–100. Entries left on a pack, time left to run on a recurring
    /// plan — both read as "how much is still mine".
    /// </summary>
    public int? CreditsPercent { get; init; }

    /// <summary>
    /// Attendance over the rolling quarter — seats pointed as attended over
    /// seats pointed at all. Null while none of theirs has been pointed: that is
    /// not a poor attendance, and the column reads "—" for it.
    /// </summary>
    public int? AttendanceRate { get; init; }

    /// <summary>
    /// Last time the member actually turned up. Not bounded by the rate's
    /// window — somebody who last came four months ago should read so.
    /// </summary>
    public DateOnly? LastVisitOn { get; init; }
}
