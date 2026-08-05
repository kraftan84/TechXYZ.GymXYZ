using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCourseTemplatesQueryHandler
    : IRequestHandler<GetCourseTemplatesQuery, CourseTemplatesPageDto>
{
    private readonly IGymDbContext _dbContext;

    public GetCourseTemplatesQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseTemplatesPageDto> Handle(
        GetCourseTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var searched = ApplySearch(
            _dbContext.CourseTemplates
                .AsNoTracking()
                .Where(template => template.IsActive),
            request.Search);

        // The chip counts follow the search, so switching format never shows a
        // count the table cannot produce. Counted over the entity: a predicate
        // written over the projected record does not translate to SQL.
        var totalCount = await searched.CountAsync(cancellationToken);
        var privateCount = await searched.CountAsync(
            template => template.Capacity == 1, cancellationToken);

        var filtered = request.Format switch
        {
            CourseFormat.Private => searched.Where(template => template.Capacity == 1),
            CourseFormat.Collective => searched.Where(template => template.Capacity > 1),
            _ => searched
        };

        var items = await filtered
            .OrderBy(template => template.Name)
            .SelectCourseTemplateListItemDto()
            .ToListAsync(cancellationToken);

        var (trailingFrom, trailingTo) = SessionStatistics.TrailingWindow(DateTime.Now);
        var byTemplate = (await SessionStatistics.LoadAsync(_dbContext, trailingFrom, trailingTo, cancellationToken))
            .GroupBy(fact => fact.CourseTemplateId)
            .ToDictionary(
                group => group.Key,
                group => (
                    FillRate: SessionStatistics.FillRate(group),
                    AttendanceRate: SessionStatistics.AttendanceRate(group)));

        var withFigures = items
            .Select(item => byTemplate.TryGetValue(item.Id, out var figures)
                ? item with { FillRate = figures.FillRate, AttendanceRate = figures.AttendanceRate }
                : item)
            .ToList();

        // "Trier : popularité" — how well the course is actually attended, which
        // is not how full it books.
        //
        // A private course is not ranked: it seats one, so it has no popularity
        // to measure, and the "Remplissage moyen" column beside it is blank for
        // the same reason — leading the ranking with a dash reads as a fault.
        // Unrated courses go last too, rather than pretending to nought.
        if (request.SortByPopularity)
        {
            withFigures =
            [
                .. withFigures
                    .OrderByDescending(item => !item.IsPrivate && item.AttendanceRate.HasValue)
                    .ThenByDescending(item => item.IsPrivate ? 0 : item.AttendanceRate ?? 0)
                    .ThenBy(item => item.Name)
            ];
        }

        return new CourseTemplatesPageDto(
            withFigures,
            totalCount,
            totalCount - privateCount,
            privateCount);
    }

    private static IQueryable<CourseTemplate> ApplySearch(IQueryable<CourseTemplate> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        return query.Where(template =>
            template.Name.Contains(term) ||
            (template.Discipline!.IsActive && template.Discipline.Name.Contains(term)));
    }
}
