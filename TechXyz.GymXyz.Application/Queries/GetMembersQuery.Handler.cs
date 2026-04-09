using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
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
            .Where(member => member.IsActive)
            .OrderBy(member => member.LastName)
            .ThenBy(member => member.FirstName)
            .SelectMemberDto(today)
            .ToListAsync(cancellationToken);
    }
}
