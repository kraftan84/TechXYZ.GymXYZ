namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The coaches grid plus the counts behind the filter chips. No paging: the
/// prototype shows a card grid without a pager, and a team is counted in tens.
/// </summary>
public sealed record CoachesPageDto(
    IReadOnlyList<CoachListItemDto> Items,
    int TotalCount,
    int AvailableCount,
    int AwayCount)
{
    public static CoachesPageDto Empty { get; } = new([], 0, 0, 0);
}

/// <summary>
/// One card of the grid. The figures whose source is the planning are nullable
/// and left unset — they render as "—" rather than being invented.
/// </summary>
public sealed record CoachListItemDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? RoleLabel,
    DateOnly JoinedOn,
    DateOnly? AwayUntil,
    List<string> Disciplines)
{
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Set by the query handler from <c>CoachStatusRules</c>.</summary>
    public CoachStatus Status { get; init; }

    /// <summary>Sessions run in the week in progress, null for a coach who runs none.</summary>
    public int? ClassesPerWeek { get; init; }

    /// <summary>Average fill of the sessions run over the trailing weeks, 0–100.</summary>
    public int? FillRate { get; init; }
}
