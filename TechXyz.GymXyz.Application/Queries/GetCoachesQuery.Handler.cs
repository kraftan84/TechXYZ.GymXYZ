using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachesQueryHandler : IRequestHandler<GetCoachesQuery, List<CoachDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCoachesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<CoachDto>> Handle(GetCoachesQuery request, CancellationToken cancellationToken)
    {
        var coaches = _unitOfWork
            .Repository<Coach, int>()
            .Entities
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
            .ToList();

        return Task.FromResult(coaches);
    }
}
