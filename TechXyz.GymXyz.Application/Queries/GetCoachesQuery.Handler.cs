using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachesQueryHandler : IRequestHandler<GetCoachesQuery, List<CoachDto>>
{
    private readonly IGymDbContext _dbContext;

    public GetCoachesQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CoachDto>> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Coaches
            .AsNoTracking()
            .OrderBy(coach => coach.LastName)
            .ThenBy(coach => coach.FirstName)
            .Select(coach => new CoachDto(
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
                        coach.Address.Country)))
            .ToListAsync(cancellationToken);
    }
}
