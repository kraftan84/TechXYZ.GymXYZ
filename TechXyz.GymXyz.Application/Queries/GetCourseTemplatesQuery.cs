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
}
