using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetSiteOptionsQueryHandler : IRequestHandler<GetSiteOptionsQuery, List<SiteOptionDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetSiteOptionsQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SiteOptionDto>> Handle(GetSiteOptionsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Sites
            .AsNoTracking()
            .Where(site => site.IsActive)
            .OrderBy(site => site.Name)
            .Select(site => new SiteOptionDto(site.Id, site.Name))
            .ToListAsync(cancellationToken);
    }
}
