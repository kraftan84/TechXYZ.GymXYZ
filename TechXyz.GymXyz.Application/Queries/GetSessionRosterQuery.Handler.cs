using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetSessionRosterQueryHandler : IRequestHandler<GetSessionRosterQuery, SessionRosterDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetSessionRosterQueryHandler(IGymDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<SessionRosterDto> Handle(GetSessionRosterQuery request, CancellationToken cancellationToken)
    {
        // Whether the screen may offer the reopen. The handler asks the same
        // question again when it is actually pressed — this one only decides
        // whether to draw the control.
        var canReopen = _currentUser.IsInRole(AttendanceRules.ReopenRole);

        var roster = await _dbContext.Sessions
            .AsNoTracking()
            .Where(session => session.Id == request.SessionId && session.IsActive)
            .Select(session => new SessionRosterDto(
                session.Id,
                session.StartsAt,
                session.EndsAt,
                session.CourseTemplate!.Name,
                session.CourseTemplateId,
                session.Coach == null ? null : session.Coach.FirstName,
                session.Coach == null ? null : session.Coach.LastName,
                session.Location!.Name,
                session.Capacity,
                session.Status == SessionStatus.Cancelled,
                session.AttendanceClosedAt,
                session.AttendanceReopenedBy,
                session.AttendanceReopenedAt,
                canReopen && session.AttendanceClosedAt != null,
                session.Registrations!
                    .Where(seat => seat.IsActive)
                    .OrderBy(seat => seat.Member!.LastName)
                    .ThenBy(seat => seat.Member!.FirstName)
                    .Select(seat => new RosterSeatDto(
                        seat.Id,
                        seat.MemberId,
                        seat.Member!.FirstName,
                        seat.Member.LastName,
                        seat.IsWaitlisted,
                        seat.Status,
                        seat.CheckedInAt))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return roster ?? SessionRosterDto.Empty;
    }
}
