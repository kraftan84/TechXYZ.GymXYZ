using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCourseTemplateDetailsPageQueryHandler
    : IRequestHandler<GetCourseTemplateDetailsPageQuery, CourseTemplateDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetCourseTemplateDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseTemplateDetailsPageDto?> Handle(
        GetCourseTemplateDetailsPageQuery request,
        CancellationToken cancellationToken)
    {
        // Projected into an anonymous shape first: the record carries pieces the
        // database knows nothing about (the empty figures, the empty session
        // list), and composing them in the projection is what fails to translate.
        var template = await _dbContext.CourseTemplates
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.CourseTemplateId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Name,
                candidate.DisciplineId,
                DisciplineName = candidate.Discipline!.Name,
                DisciplineIconKey = candidate.Discipline.IconKey,
                IconKeyOverride = candidate.IconKey,
                candidate.DurationMinutes,
                candidate.Capacity,
                candidate.DefaultLocationId,
                DefaultLocationName = candidate.DefaultLocation == null ? null : candidate.DefaultLocation.Name,
                candidate.Level,
                candidate.Intensity,
                candidate.Price,
                candidate.Description,
                Coaches = candidate.Coaches!
                    .Where(link => link.IsActive && link.Coach!.IsActive)
                    .OrderBy(link => link.Rank)
                    .Select(link => new CourseTemplateCoachDto(
                        link.Coach!.Id,
                        link.Coach.FirstName,
                        link.Coach.LastName,
                        link.Coach.RoleLabel))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return null;
        }

        return new CourseTemplateDetailsPageDto(
            template.Id,
            template.Name,
            template.DisciplineId,
            template.DisciplineName,
            template.DisciplineIconKey,
            template.IconKeyOverride,
            template.DurationMinutes,
            template.Capacity,
            template.DefaultLocationId,
            template.DefaultLocationName,
            template.Level,
            template.Intensity,
            template.Price,
            template.Description,
            template.Coaches,
            // Upcoming sessions arrive with the planning (lot 5).
            [],
            CourseTemplateStatsDto.Empty);
    }
}
