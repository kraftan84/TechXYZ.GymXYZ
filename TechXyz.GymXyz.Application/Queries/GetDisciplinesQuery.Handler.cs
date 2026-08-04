using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetDisciplinesQueryHandler : IRequestHandler<GetDisciplinesQuery, List<DisciplineDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetDisciplinesQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DisciplineDto>> Handle(GetDisciplinesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Disciplines
            .AsNoTracking()
            .Where(discipline => discipline.IsActive)
            .OrderBy(discipline => discipline.Name)
            .Select(discipline => new DisciplineDto(
                discipline.Id,
                discipline.Name,
                discipline.IconKey,
                discipline.Tone))
            .ToListAsync(cancellationToken);
    }
}
