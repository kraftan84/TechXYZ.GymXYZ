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
    private readonly ICurrentUserService _currentUser;

    public GetMemberDetailsPageQueryHandler(IGymDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<MemberDetailsPageDto?> Handle(GetMemberDetailsPageQuery request, CancellationToken cancellationToken)
    {
        var scope = CoachScope.For(_currentUser);

        // Narrowed the same way the list is, so a member nobody put on this
        // coach's roster answers "not found" rather than opening by URL.
        var member = await scope
            .ApplyToMembers(_dbContext.Members.AsNoTracking())
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

        var covers = await _dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription => subscription.IsActive && subscription.MemberId == request.MemberId)
            .OrderByDescending(subscription => subscription.EndsOn)
            .Select(subscription => new SubscriptionCoverDto(
                subscription.Id,
                subscription.PlanId,
                subscription.Plan!.Name,
                subscription.Plan.Kind,
                subscription.StartedOn,
                subscription.EndsOn,
                subscription.CreditsRemaining,
                subscription.CreditsTotal,
                subscription.PriceLabel,
                subscription.AutoRenew,
                subscription.Price,
                subscription.Payments!
                    .Where(payment => payment.IsActive && payment.Status == PaymentStatus.Collected)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m,
                subscription.Payments!.Any(payment =>
                    payment.IsActive && payment.Status != PaymentStatus.Collected)))
            .ToListAsync(cancellationToken);

        var subscriptions = covers
            .Select(cover => new MemberSubscriptionDto(
                cover.SubscriptionId,
                cover.PlanId,
                cover.PlanName,
                cover.Kind,
                cover.StartedOn,
                cover.EndsOn,
                cover.CreditsRemaining,
                cover.CreditsTotal,
                cover.PriceLabel,
                cover.AutoRenew,
                cover.StartedOn > today
                    ? null
                    : SubscriptionStatusRules.Resolve(cover, today, horizon)))
            .ToList();

        // The record and the row on the members table read the same cover,
        // through the same rule — a record that disagreed with the list it was
        // opened from would be worse than either.
        //
        // "En cours" means in force, so a cover that has run out is not one:
        // the card falls back to "Aucun abonnement en cours" and offers to sell
        // one, which is exactly what somebody looking at Théo Garnier needs.
        var governing = SubscriptionStatusRules.Governing(covers, today, horizon);
        var currentSubscription = governing is null
            || SubscriptionStatusRules.Resolve(governing, today, horizon)
                is SubscriptionStatus.Late or SubscriptionStatus.Ended
            ? null
            : subscriptions.FirstOrDefault(subscription => subscription.Id == governing.SubscriptionId);

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.IsActive && payment.MemberId == request.MemberId)
            .OrderByDescending(payment => payment.Date)
            .ThenByDescending(payment => payment.Id)
            .Select(payment => new MemberPaymentDto(
                payment.Id,
                payment.Date,
                payment.Label,
                payment.Amount,
                payment.Method,
                payment.Status))
            .ToListAsync(cancellationToken);

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
                        .Count(seat => seat.IsActive && !seat.IsWaitlisted)),
                registration.Status))
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
        var (attendanceFrom, attendanceTo) = SessionStatistics.AttendanceWindow(now);
        var attendance = await MemberAttendance.LoadAsync(
            _dbContext, [member.Id], attendanceFrom, attendanceTo, cancellationToken);

        // The same two figures as the row on the members table, from the same
        // helper — a record that disagreed with the list it was opened from
        // would be worse than either.
        var fact = attendance.GetValueOrDefault(member.Id);
        var stats = new MemberStatsDto(pastSessions.Count, fact?.Rate, fact?.LastVisitOnDate);

        // What a coach is shown of somebody they teach: enough to reach them and
        // to know whether they are covered. Not their address, not their date of
        // birth, not the gym's notes about them, and not what they have paid —
        // /abonnements is closed to a coach, and a fiche must not be the way
        // round it.
        //
        // Dropped here rather than in the markup: a value the browser never
        // receives cannot be read off the page.
        if (scope.IsRestricted)
        {
            // The cover keeps its plan, its dates and its credits — that is what
            // "is this person entitled to be in my class" is made of — but not
            // its price. PriceLabel is the one field on it that is money.
            var cover = currentSubscription is null
                ? null
                : currentSubscription with { PriceLabel = string.Empty };

            return new MemberDetailsPageDto(
                member.Id,
                member.FirstName,
                member.LastName,
                member.Email,
                member.Phone,
                member.JoinedOn,
                BirthDate: null,
                Notes: null,
                Address: null,
                MemberStatusRules.Resolve(covers, today, horizon),
                cover,
                Subscriptions: [],
                upcomingSessions,
                pastSessions,
                Payments: [],
                stats);
        }

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
            MemberStatusRules.Resolve(covers, today, horizon),
            currentSubscription,
            subscriptions,
            upcomingSessions,
            pastSessions,
            payments,
            stats);
    }
}
