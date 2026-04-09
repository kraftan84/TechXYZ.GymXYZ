using MediatR;
using Microsoft.EntityFrameworkCore;
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
                lesson.EndDate < now ? MemberLessonStatus.Completed : MemberLessonStatus.Confirmed,
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
                lesson.EndDate < now ? MemberLessonStatus.Completed : MemberLessonStatus.Confirmed,
                lesson.MaxParticipants,
                Math.Max(0, lesson.MaxParticipants - lesson.Participants!.Count(participant => participant.IsActive))))
            .ToListAsync(cancellationToken);

        var lessons = privateLessons
            .Concat(collectiveLessons)
            .OrderByDescending(lesson => lesson.StartDate)
            .ToList();

        var completedLessonsCount = lessons.Count(lesson => lesson.Status == MemberLessonStatus.Completed);
        var totalLessonsCount = lessons.Count;
        var attendanceRate = totalLessonsCount == 0
            ? 0
            : (int)Math.Round(completedLessonsCount * 100d / totalLessonsCount);
        var lastVisit = lessons
            .Where(lesson => lesson.EndDate <= now)
            .OrderByDescending(lesson => lesson.EndDate)
            .Select(lesson => DateOnly.FromDateTime(lesson.EndDate))
            .FirstOrDefault();
        var hasLastVisit = lessons.Any(lesson => lesson.EndDate <= now);

        var activeSubscription = subscriptions.FirstOrDefault(subscription => subscription.Status == MemberSubscriptionStatus.Active);
        var subscriptionRemainingPercent = activeSubscription is null
            ? 0
            : GetSubscriptionRemainingPercent(activeSubscription, today);

        var stats = new MemberStatsDto(
            totalLessonsCount,
            attendanceRate,
            hasLastVisit ? lastVisit : null,
            subscriptionRemainingPercent);

        return new MemberDetailsPageDto(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.Address,
            stats,
            subscriptions,
            lessons);
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

    private static int GetSubscriptionRemainingPercent(MemberSubscriptionDto subscription, DateOnly today)
    {
        var totalDays = Math.Max(1, subscription.EndDate.DayNumber - subscription.StartDate.DayNumber + 1);
        var remainingDays = Math.Max(0, subscription.EndDate.DayNumber - today.DayNumber + 1);

        return (int)Math.Round(remainingDays * 100d / totalDays);
    }
}
