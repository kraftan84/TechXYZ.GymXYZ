using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Components.Features.Planning;

/// <summary>
/// What the toolbar chips currently narrow the grid to. A plain holder passed
/// down to the presentations, which never change it — the page does, then
/// re-queries.
/// </summary>
public sealed class PlanningFilterState
{
    public int? CoachId { get; set; }
    public int? LocationId { get; set; }
    public CourseFormat? Format { get; set; }

    public bool IsFiltered => CoachId is not null || LocationId is not null || Format is not null;
}
