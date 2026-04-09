using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachByIdQueryHandler : IRequestHandler<GetCoachByIdQuery, CoachDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetCoachByIdQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CoachDto?> Handle(GetCoachByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Coaches
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id && candidate.IsActive)
            .SelectCoachDto()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
