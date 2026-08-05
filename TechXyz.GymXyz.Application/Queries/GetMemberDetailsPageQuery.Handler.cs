using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMemberDetailsPageQueryHandler : IRequestHandler<GetMemberDetailsPageQuery, MemberDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetMemberDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MemberDetailsPageDto?> Handle(GetMemberDetailsPageQuery request, CancellationToken cancellationToken)
    {
        var member = await _dbContext.Members
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.MemberId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.JoinedOn,
                candidate.BirthDate,
                candidate.Notes,
                Address = candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (member is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var horizon = MemberStatusRules.HorizonFrom(today);
        var now = DateTime.Now;

        var subscriptionsRaw = await _dbContext.Members
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.MemberId && candidate.IsActive)
            .SelectMany(candidate => candidate.Subscriptions!
                .Where(subscription => subscription.IsActive)
                .Select(subscription => new
                {
                    subscription.Id,
                    subscription.StartDate,
                    subscription.EndDate,
                    subscription.NumberOfSessions
                }))
            .OrderByDescending(subscription => subscription.EndDate)
            .ToListAsync(cancellationToken);

        var subscriptions = subscriptionsRaw
            .Select(subscription =>
            {
                var status = GetSubscriptionStatus(subscription.StartDate, subscription.EndDate, today);
                var sessionsRemaining = status == MemberSubscriptionStatus.Active
                    ? Math.Max(0, subscription.NumberOfSessions)
                    : 0;

                return new MemberSubscriptionDto(
                    subscription.Id,
                    subscription.StartDate,
                    subscription.EndDate,
                    subscription.NumberOfSessions,
                    sessionsRemaining,
                    status);
            })
            .ToList();

        // The standing reads the same value as the list: the latest end date
        // among the subscriptions covering today.
        var currentSubscription = subscriptions
            .Where(subscription => subscription.StartDate <= today && subscription.EndDate >= today)
            .MaxBy(subscription => subscription.EndDate);

        // A seat on the waiting list is still a seat the member holds, so it is
        // listed here — it is only occupancy that ignores it.
        var sessions = await _dbContext.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.IsActive &&
                registration.MemberId == request.MemberId &&
                registration.Session!.IsActive &&
                registration.Session.Status != SessionStatus.Cancelled)
            .Select(registration => new MemberSessionDto(
                registration.Session!.Id,
                registration.Session.CourseTemplate!.Name,
                registration.Session.StartsAt,
                registration.Session.EndsAt,
                registration.Session.Coach == null ? null : registration.Session.Coach.FirstName,
                registration.Session.Coach == null ? null : registration.Session.Coach.LastName,
                registration.Session.Capacity,
                Math.Max(
                    0,
                    registration.Session.Capacity - registration.Session.Registrations!
                        .Count(seat => seat.IsActive && !seat.IsWaitlisted))))
            .ToListAsync(cancellationToken);

        var upcomingSessions = sessions
            .Where(session => session.StartsAt >= now)
            .OrderBy(session => session.StartsAt)
            .ToList();

        var pastSessions = sessions
            .Where(session => session.StartsAt < now)
            .OrderByDescending(session => session.StartsAt)
            .ToList();

        // "Séances · depuis l'inscription" counts what the member has already
        // been to, so the seats booked for the weeks ahead stay out of it.
        //
        // Attendance rate and last visit come from check-in (lot 6): left unset
        // rather than approximated from the schedule.
        var stats = new MemberStatsDto(pastSessions.Count, AttendanceRate: null, LastVisitOn: null);

        return new MemberDetailsPageDto(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.JoinedOn,
            member.BirthDate,
            member.Notes,
            member.Address,
            MemberStatusRules.Resolve(currentSubscription?.EndDate, horizon),
            currentSubscription,
            subscriptions,
            upcomingSessions,
            pastSessions,
            // Payments arrive at lot 7 (Abonnements & encaissements).
            [],
            stats);
    }

    private static MemberSubscriptionStatus GetSubscriptionStatus(DateOnly startDate, DateOnly endDate, DateOnly today)
    {
        if (endDate < today)
        {
            return MemberSubscriptionStatus.Expired;
        }

        if (startDate > today)
        {
            return MemberSubscriptionStatus.Paused;
        }

        return MemberSubscriptionStatus.Active;
    }
}
