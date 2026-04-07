using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetMembersQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _dbContext.Members
            .AsNoTracking()
            .OrderBy(member => member.LastName)
            .ThenBy(member => member.FirstName)
            .Select(member => new MemberDto(
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
                        member.Address.Country)))
            .ToListAsync(cancellationToken);
    }
}
