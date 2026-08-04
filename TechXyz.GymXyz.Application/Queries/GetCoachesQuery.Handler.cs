using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachesQueryHandler : IRequestHandler<GetCoachesQuery, CoachesPageDto>
{
    private readonly IGymDbContext _dbContext;

    public GetCoachesQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CoachesPageDto> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var searched = ApplySearch(
            _dbContext.Coaches
                .AsNoTracking()
                .Where(coach => coach.IsActive),
            request.Search);

        // The chip counts follow the search, so switching standing never shows
        // a count the grid cannot produce.
        var totalCount = await searched.CountAsync(cancellationToken);
        var awayCount = await searched.CountAsync(
            CoachStatusRules.Matches(CoachStatus.Away, today), cancellationToken);

        var filtered = request.Status is { } status
            ? searched.Where(CoachStatusRules.Matches(status, today))
            : searched;

        var items = await filtered
            .OrderBy(coach => coach.LastName)
            .ThenBy(coach => coach.FirstName)
            .SelectCoachListItemDto()
            .ToListAsync(cancellationToken);

        return new CoachesPageDto(
            items
                .Select(item => item with
                {
                    Status = CoachStatusRules.Resolve(item.AwayUntil, today)
                })
                .ToList(),
            totalCount,
            totalCount - awayCount,
            awayCount);
    }

    private static IQueryable<Coach> ApplySearch(IQueryable<Coach> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        return query.Where(coach =>
            coach.FirstName.Contains(term) ||
            coach.LastName.Contains(term) ||
            (coach.RoleLabel != null && coach.RoleLabel.Contains(term)) ||
            (coach.Email != null && coach.Email.Contains(term)) ||
            coach.Disciplines!.Any(link =>
                link.IsActive &&
                link.Discipline!.IsActive &&
                link.Discipline.Name.Contains(term)));
    }
}
