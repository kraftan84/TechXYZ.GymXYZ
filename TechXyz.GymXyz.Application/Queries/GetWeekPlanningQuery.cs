using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The week grid. <paramref name="WeekStart"/> is normalised to the Monday of
/// the week it falls in, so the caller can hand over any day of the week it
/// wants to show.
/// <para>
/// The three filters are the chips of the toolbar, all applied server-side: the
/// grid never receives sessions it will not draw.
/// </para>
/// </summary>
public sealed class GetWeekPlanningQuery : IRequest<WeekPlanningDto>
{
    public GetWeekPlanningQuery(DateOnly weekStart)
    {
        WeekStart = weekStart;
    }

    public DateOnly WeekStart { get; }

    /// <summary>Null keeps every coach ("Tous les coachs").</summary>
    public int? CoachId { get; init; }

    /// <summary>Null keeps every venue ("Tous les lieux").</summary>
    public int? LocationId { get; init; }

    /// <summary>Null keeps both formats.</summary>
    public CourseFormat? Format { get; init; }
}
