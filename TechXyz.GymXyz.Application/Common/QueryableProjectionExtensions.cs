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
            tenant.MarkKind,
            tenant.LogoPath,
            tenant.LogoDarkPath,
            tenant.CircleLogo,
            tenant.WordmarkText,
            tenant.WordmarkPrefix,
            tenant.WordmarkAccent,
            tenant.IsSolo)
        {
            City = tenant.City
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
    /// Rows of the members table. <c>CurrentSubscriptionEndsOn</c> is the latest
    /// end date among the subscriptions covering <paramref name="today"/> —
    /// the single value the standing rule reads.
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
                .Where(subscription =>
                    subscription.IsActive &&
                    subscription.StartDate <= today &&
                    subscription.EndDate >= today)
                .Max(subscription => (DateOnly?)subscription.EndDate)));
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
                subscription.StartDate <= today && subscription.EndDate >= today),
            member.Address == null
                ? null
                : new AddressDto(
                    member.Address.Street,
                    member.Address.ZipCode,
                    member.Address.City,
                    member.Address.Country)));
    }
}
