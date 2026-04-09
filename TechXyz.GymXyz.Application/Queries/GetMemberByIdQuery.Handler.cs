using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetMemberByIdQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MemberDto?> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _dbContext.Members
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id && candidate.IsActive)
            .SelectMemberDto(today)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
