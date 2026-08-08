using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class QueryableProjectionExtensions
{
    public static IQueryable<TenantBrandDto> SelectTenantBrandDto(this IQueryable<Tenant> query)
    {
        return query.Select(tenant => new TenantBrandDto(
            tenant.Id,
            tenant.Slug,
            tenant.ThemeKey,
            tenant.DisplayName,
            tenant.Baseline,
            tenant.LogoPath,
            tenant.LogoDarkPath,
            tenant.CircleLogo,
            tenant.WordmarkText,
            tenant.WordmarkPrefix,
            tenant.WordmarkAccent,
            tenant.IsSolo)
        {
            City = tenant.City,
            AreaLabel = tenant.AreaLabel,
            ZipCode = tenant.ZipCode,
            ShowSchoolVacations = tenant.ShowSchoolVacations,
            IsSuspended = tenant.IsSuspended
        });
    }

    public static IQueryable<CoachDto> SelectCoachDto(this IQueryable<Coach> query)
    {
        return query.Select(coach => new CoachDto(
            coach.Id,
            coach.FirstName,
            coach.LastName,
            coach.Email,
            coach.Phone,
            coach.Address == null
                ? null
                : new AddressDto(
                    coach.Address.Street,
                    coach.Address.ZipCode,
                    coach.Address.City,
                    coach.Address.Country)));
    }

    /// <summary>
    /// Cards of the coaches grid. Disciplines come out in display order — the
    /// first pill is the one tinted in the brand colour.
    /// </summary>
    public static IQueryable<CoachListItemDto> SelectCoachListItemDto(this IQueryable<Coach> query)
    {
        return query.Select(coach => new CoachListItemDto(
            coach.Id,
            coach.FirstName,
            coach.LastName,
            coach.Email,
            coach.Phone,
            coach.RoleLabel,
            coach.JoinedOn,
            coach.AwayUntil,
            coach.Disciplines!
                .Where(link => link.IsActive && link.Discipline!.IsActive)
                .OrderBy(link => link.Rank)
                .Select(link => link.Discipline!.Name)
                .ToList()));
    }

    /// <summary>
    /// Rows of the course catalogue. Coaches come out in display order — that is
    /// the order their avatars stack in.
    /// </summary>
    public static IQueryable<CourseTemplateListItemDto> SelectCourseTemplateListItemDto(
        this IQueryable<CourseTemplate> query)
    {
        return query.Select(template => new CourseTemplateListItemDto(
            template.Id,
            template.Name,
            template.Discipline!.Name,
            template.Discipline.IconKey,
            template.IconKey,
            template.DurationMinutes,
            template.Capacity,
            template.Level,
            template.Intensity,
            template.Coaches!
                .Where(link => link.IsActive && link.Coach!.IsActive)
                .OrderBy(link => link.Rank)
                .Select(link => new CourseTemplateCoachDto(
                    link.Coach!.Id,
                    link.Coach.FirstName,
                    link.Coach.LastName,
                    link.Coach.RoleLabel))
                .ToList()));
    }

    /// <summary>
    /// Cards of the venue catalogue. Equipment comes out in display order — the
    /// card shows the first four and counts the rest.
    /// </summary>
    public static IQueryable<LocationListItemDto> SelectLocationListItemDto(this IQueryable<Location> query)
    {
        return query.Select(location => new LocationListItemDto(
            location.Id,
            location.Name,
            location.Kind,
            location.TypeLabel,
            location.IconKey,
            location.Tone,
            location.Capacity,
            location.AreaSqm,
            location.Floor,
            location.IsOpenAccess,
            location.IsWeatherDependent,
            location.FallbackLocation == null ? null : location.FallbackLocation.Name,
            location.Address == null
                ? null
                : new AddressDto(
                    location.Address.Street,
                    location.Address.ZipCode,
                    location.Address.City,
                    location.Address.Country),
            location.Equipment!
                .Where(equipment => equipment.IsActive)
                .OrderBy(equipment => equipment.Rank)
                .Select(equipment => equipment.Label)
                .ToList()));
    }

    /// <summary>
    /// Cards of the formules grid. The member count is people covered by the
    /// plan right now — the card says "64 membres", not "64 ever bought it".
    /// <para>
    /// Shared by <c>GetPlansQuery</c> and the abonnements overview so the cards
    /// and the plan picker cannot end up counting differently.
    /// </para>
    /// </summary>
    public static IQueryable<PlanDto> SelectPlanDto(this IQueryable<Plan> query, DateOnly today)
    {
        return query.Select(plan => new PlanDto(
            plan.Id,
            plan.Name,
            plan.ShortName,
            plan.Price,
            plan.Unit,
            plan.Kind,
            plan.CreditCount,
            plan.ValidityMonths,
            plan.BillingLabel,
            plan.Description,
            plan.Tone,
            plan.IsFeatured,
            plan.Rank,
            plan.Subscriptions!.Count(subscription =>
                subscription.IsActive &&
                subscription.StartedOn <= today &&
                subscription.EndsOn >= today)));
    }

    /// <summary>
    /// Rows of the members table, each with the subscriptions that have started —
    /// the facts the standing rule reads.
    /// <para>
    /// The covers come out as a list rather than as one pre-picked row because
    /// which one governs is a decision of <c>SubscriptionStatusRules</c>, and a
    /// member holding a monthly plan and a pack at once has two. In practice
    /// that is one or two rows apiece over a page of at most two hundred.
    /// </para>
    /// <para>
    /// Subscriptions not yet started are left out on purpose: a renewal booked
    /// for next month is not a cover today, and counting it would have the member
    /// reading "Actif" on something that has not begun.
    /// </para>
    /// </summary>
    public static IQueryable<MemberListItemDto> SelectMemberListItemDto(this IQueryable<Member> query, DateOnly today)
    {
        return query.Select(member => new MemberListItemDto(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.JoinedOn,
            member.Subscriptions!
                .Where(subscription => subscription.IsActive && subscription.StartedOn <= today)
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
                .ToList()));
    }

    public static IQueryable<MemberDto> SelectMemberDto(this IQueryable<Member> query, DateOnly today)
    {
        return query.Select(member => new MemberDto(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.Subscriptions!.Any(subscription =>
                subscription.StartedOn <= today && subscription.EndsOn >= today),
            member.Address == null
                ? null
                : new AddressDto(
                    member.Address.Street,
                    member.Address.ZipCode,
                    member.Address.City,
                    member.Address.Country)));
    }
}
