using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLocationOptionsQueryHandler : IRequestHandler<GetLocationOptionsQuery, List<LocationOptionDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetLocationOptionsQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LocationOptionDto>> Handle(GetLocationOptionsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .Select(location => new LocationOptionDto(location.Id, location.Name))
            .ToListAsync(cancellationToken);
    }
}
