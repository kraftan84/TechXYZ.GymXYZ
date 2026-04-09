using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class QueryableProjectionExtensions
{
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
