using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>Everything the course record shows. Null when there is no such course.</summary>
public sealed class GetCourseTemplateDetailsPageQuery : IRequest<CourseTemplateDetailsPageDto?>
{
    public GetCourseTemplateDetailsPageQuery(int courseTemplateId)
    {
        CourseTemplateId = courseTemplateId;
    }

    public int CourseTemplateId { get; }
}
