using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Everything the Accueil draws, in one read: the week strip, today's classes
/// and the three alerts.
/// <para>
/// There are no KPI here, and that is a decision rather than an omission. The
/// hand-off asks for four; neither <c>screen-accueil.jsx</c> nor its mobile
/// counterpart draws one, and both describe the same planning-first screen — so
/// the mockup was followed.
/// </para>
/// </summary>
public sealed record DashboardDto(
    DateOnly Today,
    DateOnly WeekStart,
    IReadOnlyList<DashboardDayDto> Week,
    IReadOnlyList<DashboardClassDto> TodayClasses,
    DashboardAlertsDto Alerts,
    int WeekCoachCount)
{
    public static DashboardDto Empty { get; } =
        new(default, default, [], [], DashboardAlertsDto.Empty, 0);

    public DateOnly WeekEnd => WeekStart.AddDays(6);

    /// <summary>Classes across the seven days — the figure beside the week's dates.</summary>
    public int WeekSessionCount => Week.Sum(day => day.SessionCount);
}

/// <summary>One cell of the week strip.</summary>
public sealed record DashboardDayDto(DateOnly Date, int SessionCount, bool IsToday);

/// <summary>
/// One row of « Aujourd'hui ». The same fields the planning projects, so the two
/// screens describe a class the same way.
/// </summary>
public sealed record DashboardClassDto(
    int SessionId,
    DateTime StartsAt,
    DateTime EndsAt,
    string CourseName,
    string? CoachFirstName,
    string? CoachLastName,
    string LocationName,
    int Registered,
    int Capacity)
{
    /// <summary>A session that seats one is private — the same rule the catalogue applies.</summary>
    public bool IsPrivate => Capacity == 1;

    public bool IsFull => Registered >= Capacity && Capacity > 1;

    public int? FillPercent => PlanningRules.FillRate(Registered, Capacity);

    /// <summary>"Studio A · Nora Lemoine", or the venue alone for an unstaffed slot.</summary>
    public string Meta => CoachFullName is null
        ? LocationName
        : $"{LocationName} · {CoachFullName}";

    public string? CoachFullName => CoachLastName is null
        ? null
        : $"{CoachFirstName} {CoachLastName}".Trim();
}

/// <summary>
/// The three things « À surveiller » raises. Every one of them is counted by a
/// rule that was decided and tested in an earlier lot — this lot reuses them and
/// invents none, which is why it was moved to the end of the programme.
/// </summary>
/// <param name="ExpiringCount">
/// Members whose cover reads <see cref="SubscriptionStatus.ExpiringSoon"/>, off
/// <see cref="SubscriptionStanding"/> — the rows the Abonnements suivi shows.
/// </param>
/// <param name="LateCount">Members whose cover reads <see cref="SubscriptionStatus.Late"/>.</param>
/// <param name="LateAmount">What those covers are worth — « 180 € à encaisser ».</param>
/// <param name="SheetsToPoint">
/// Open attendance sheets, off <see cref="AttendanceRules.OpenSheets"/> — the
/// same rows the Présences screen lists and the same figure its KPI shows.
/// </param>
public sealed record DashboardAlertsDto(
    int ExpiringCount,
    int LateCount,
    decimal LateAmount,
    int SheetsToPoint)
{
    public static DashboardAlertsDto Empty { get; } = new(0, 0, 0m, 0);

    /// <summary>
    /// What the Abonnements badge carries: covers that need somebody to act.
    /// The prototype's sidebar shows 6 against its 4 expiring and 2 late.
    /// </summary>
    public int SubscriptionsToWatch => ExpiringCount + LateCount;
}
