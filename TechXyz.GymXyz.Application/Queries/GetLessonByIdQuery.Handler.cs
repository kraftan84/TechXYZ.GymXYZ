using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDetailsDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetLessonByIdQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LessonDetailsDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var privateLesson = await _dbContext.PrivateLessons
            .AsNoTracking()
            .Where(lesson => lesson.Id == request.Id && lesson.IsActive && lesson.Coach.IsActive && lesson.Location.IsActive)
            .Select(lesson => new
            {
                lesson.Id,
                lesson.Name,
                lesson.Description,
                lesson.Type,
                ThemeId = lesson.Theme != null && lesson.Theme.IsActive ? lesson.Theme.Id : (int?)null,
                ThemeName = lesson.Theme != null && lesson.Theme.IsActive ? lesson.Theme.Name : null,
                CoachId = lesson.Coach.Id,
                CoachFirstName = lesson.Coach.FirstName,
                CoachLastName = lesson.Coach.LastName,
                lesson.StartDate,
                lesson.EndDate,
                LocationId = lesson.Location.Id,
                LocationName = lesson.Location.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (privateLesson is not null)
        {
            return new LessonDetailsDto(
                privateLesson.Id,
                privateLesson.Name,
                privateLesson.Description,
                privateLesson.Type,
                privateLesson.ThemeId,
                privateLesson.ThemeName,
                privateLesson.CoachId,
                privateLesson.CoachFirstName,
                privateLesson.CoachLastName,
                privateLesson.StartDate,
                privateLesson.EndDate,
                [new LocationOptionDto(privateLesson.LocationId, privateLesson.LocationName)],
                null);
        }

        return await _dbContext.CollectiveLessons
            .AsNoTracking()
            .Where(lesson => lesson.Id == request.Id && lesson.IsActive && lesson.Coach.IsActive)
            .Select(lesson => new LessonDetailsDto(
                lesson.Id,
                lesson.Name,
                lesson.Description,
                lesson.Type,
                lesson.Theme != null && lesson.Theme.IsActive ? lesson.Theme.Id : null,
                lesson.Theme != null && lesson.Theme.IsActive ? lesson.Theme.Name : null,
                lesson.Coach.Id,
                lesson.Coach.FirstName,
                lesson.Coach.LastName,
                lesson.StartDate,
                lesson.EndDate,
                lesson.Locations
                    .Where(location => location.IsActive)
                    .OrderBy(location => location.Name)
                    .Select(location => new LocationOptionDto(location.Id, location.Name))
                    .ToList(),
                lesson.MaxParticipants))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
