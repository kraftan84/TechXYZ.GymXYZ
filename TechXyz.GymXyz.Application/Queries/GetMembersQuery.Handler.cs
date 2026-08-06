using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, MembersPageDto>
{
    private readonly IGymDbContext _dbContext;

    public GetMembersQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MembersPageDto> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var horizon = MemberStatusRules.HorizonFrom(today);

        var searched = ApplySearch(
            _dbContext.Members
                .AsNoTracking()
                .Where(member => member.IsActive),
            request.Search);

        // The chip counts follow the search, so switching standing never shows a
        // count the list cannot produce.
        var totalCount = await searched.CountAsync(cancellationToken);
        var expiringSoonCount = await searched.CountAsync(
            MemberStatusRules.Matches(MemberStatus.ExpiringSoon, today, horizon), cancellationToken);
        var inactiveCount = await searched.CountAsync(
            MemberStatusRules.Matches(MemberStatus.Inactive, today, horizon), cancellationToken);

        var filtered = request.Status is { } status
            ? searched.Where(MemberStatusRules.Matches(status, today, horizon))
            : searched;

        var filteredCount = request.Status is null
            ? totalCount
            : await filtered.CountAsync(cancellationToken);

        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var page = Math.Max(1, request.Page);

        var items = await filtered
            .OrderBy(member => member.LastName)
            .ThenBy(member => member.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .SelectMemberListItemDto(today)
            .ToListAsync(cancellationToken);

        // Assiduité and last visit, for the page on screen only: the window is a
        // quarter and the table pages at 200, so this stays one small query
        // rather than one per row.
        var (attendanceFrom, attendanceTo) = SessionStatistics.AttendanceWindow(DateTime.Now);
        var attendance = await MemberAttendance.LoadAsync(
            _dbContext,
            [.. items.Select(item => item.Id)],
            attendanceFrom,
            attendanceTo,
            cancellationToken);

        return new MembersPageDto(
            items
                .Select(item =>
                {
                    var cover = SubscriptionStatusRules.Governing(item.Covers, today, horizon);

                    return item with
                    {
                        Status = MemberStatusRules.Resolve(item.Covers, today, horizon),
                        GoverningCover = cover,
                        PlanLabel = cover?.PlanName,
                        CreditsLabel = cover?.CreditsLabel,
                        CreditsPercent = cover?.CreditsPercent,
                        AttendanceRate = attendance.GetValueOrDefault(item.Id)?.Rate,
                        LastVisitOn = attendance.GetValueOrDefault(item.Id)?.LastVisitOnDate
                    };
                })
                .ToList(),
            totalCount,
            totalCount - expiringSoonCount - inactiveCount,
            expiringSoonCount,
            inactiveCount,
            filteredCount);
    }

    private static IQueryable<Member> ApplySearch(IQueryable<Member> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        return query.Where(member =>
            member.FirstName.Contains(term) ||
            member.LastName.Contains(term) ||
            (member.Email != null && member.Email.Contains(term)) ||
            (member.Phone != null && member.Phone.Contains(term)));
    }
}
