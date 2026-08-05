using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLessonsPageQueryHandler : IRequestHandler<GetLessonsPageQuery, LessonsPageDto>
{
    private readonly IGymDbContext _dbContext;

    public GetLessonsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LessonsPageDto> Handle(GetLessonsPageQuery request, CancellationToken cancellationToken)
    {
        var privateLessonsRaw = await _dbContext.PrivateLessons
            .AsNoTracking()
            .Where(lesson => lesson.IsActive && lesson.Coach.IsActive && lesson.Location.IsActive)
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
            .ToListAsync(cancellationToken);

        var collectiveLessons = await _dbContext.CollectiveLessons
            .AsNoTracking()
            .Where(lesson => lesson.IsActive && lesson.Coach.IsActive)
            .Select(lesson => new LessonListItemDto(
                lesson.Id,
                lesson.Name,
                lesson.Description,
                lesson.Type,
                lesson.Theme != null && lesson.Theme.IsActive ? lesson.Theme.Id : (int?)null,
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
            .ToListAsync(cancellationToken);

        var themes = await _dbContext.LessonThemes
            .AsNoTracking()
            .Where(theme => theme.IsActive)
            .OrderBy(theme => theme.Name)
            .Select(theme => new LessonThemeDto(theme.Id, theme.Name, theme.Description))
            .ToListAsync(cancellationToken);

        var coaches = await _dbContext.Coaches
            .AsNoTracking()
            .Where(coach => coach.IsActive)
            .OrderBy(coach => coach.LastName)
            .ThenBy(coach => coach.FirstName)
            .Select(coach => new LessonCoachDto(coach.Id, coach.FirstName, coach.LastName))
            .ToListAsync(cancellationToken);

        var locations = await _dbContext.Locations
            .AsNoTracking()
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .Select(location => new LocationOptionDto(location.Id, location.Name))
            .ToListAsync(cancellationToken);

        var privateLessons = privateLessonsRaw
            .Select(lesson => new LessonListItemDto(
                lesson.Id,
                lesson.Name,
                lesson.Description,
                lesson.Type,
                lesson.ThemeId,
                lesson.ThemeName,
                lesson.CoachId,
                lesson.CoachFirstName,
                lesson.CoachLastName,
                lesson.StartDate,
                lesson.EndDate,
                [new LocationOptionDto(lesson.LocationId, lesson.LocationName)],
                null))
            .ToList();

        var lessons = privateLessons
            .Concat(collectiveLessons)
            .OrderBy(lesson => lesson.StartDate)
            .ThenBy(lesson => lesson.Name)
            .ToList();

        return new LessonsPageDto(
            lessons,
            themes,
            coaches,
            locations);
    }
}
