using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

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
                    subscription.NumberOfLessons
                }))
            .OrderByDescending(subscription => subscription.EndDate)
            .ToListAsync(cancellationToken);

        var subscriptions = subscriptionsRaw
            .Select(subscription =>
            {
                var status = GetSubscriptionStatus(subscription.StartDate, subscription.EndDate, today);
                var lessonsRemaining = status == MemberSubscriptionStatus.Active
                    ? Math.Max(0, subscription.NumberOfLessons)
                    : 0;

                return new MemberSubscriptionDto(
                    subscription.Id,
                    subscription.StartDate,
                    subscription.EndDate,
                    subscription.NumberOfLessons,
                    lessonsRemaining,
                    status);
            })
            .ToList();

        // The standing reads the same value as the list: the latest end date
        // among the subscriptions covering today.
        var currentSubscription = subscriptions
            .Where(subscription => subscription.StartDate <= today && subscription.EndDate >= today)
            .MaxBy(subscription => subscription.EndDate);

        var privateLessons = await _dbContext.PrivateLessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.IsActive &&
                lesson.Coach.IsActive &&
                lesson.Member != null &&
                lesson.Member.IsActive &&
                lesson.Member.Id == request.MemberId)
            .Select(lesson => new MemberLessonDto(
                lesson.Id,
                lesson.Name,
                lesson.Type,
                lesson.StartDate,
                lesson.EndDate,
                lesson.Coach.FirstName,
                lesson.Coach.LastName,
                1,
                0))
            .ToListAsync(cancellationToken);

        var collectiveLessons = await _dbContext.CollectiveLessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.IsActive &&
                lesson.Coach.IsActive &&
                lesson.Participants!.Any(participant => participant.IsActive && participant.Id == request.MemberId))
            .Select(lesson => new MemberLessonDto(
                lesson.Id,
                lesson.Name,
                lesson.Type,
                lesson.StartDate,
                lesson.EndDate,
                lesson.Coach.FirstName,
                lesson.Coach.LastName,
                lesson.MaxParticipants,
                Math.Max(0, lesson.MaxParticipants - lesson.Participants!.Count(participant => participant.IsActive))))
            .ToListAsync(cancellationToken);

        var lessons = privateLessons.Concat(collectiveLessons).ToList();

        var upcomingLessons = lessons
            .Where(lesson => lesson.StartDate >= now)
            .OrderBy(lesson => lesson.StartDate)
            .ToList();

        var pastLessons = lessons
            .Where(lesson => lesson.StartDate < now)
            .OrderByDescending(lesson => lesson.StartDate)
            .ToList();

        // Attendance rate and last visit come from check-in (lot 6): left unset
        // rather than approximated from the schedule.
        var stats = new MemberStatsDto(lessons.Count, AttendanceRate: null, LastVisitOn: null);

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
            upcomingLessons,
            pastLessons,
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
