using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Course catalogue: free-text search and format filter, both server-side. No
/// paging — the prototype draws a plain table without a pager.
/// </summary>
public sealed class GetCourseTemplatesQuery : IRequest<CourseTemplatesPageDto>
{
    /// <summary>Matches the course name or its discipline.</summary>
    public string? Search { get; init; }

    /// <summary>Null keeps every format ("Tous").</summary>
    public CourseFormat? Format { get; init; }

    /// <summary>
    /// The "Trier : popularité" chip. False keeps the default alphabetical
    /// order. Popularity reads the attendance rate, not the fill: a course that
    /// books out and empties on the night is not the popular one.
    /// </summary>
    public bool SortByPopularity { get; init; }
}
