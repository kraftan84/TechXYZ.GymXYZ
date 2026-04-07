using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachByIdQueryHandler : IRequestHandler<GetCoachByIdQuery, CoachDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCoachByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<CoachDto?> Handle(GetCoachByIdQuery request, CancellationToken cancellationToken)
    {
        var coach = _unitOfWork
            .Repository<Coach, int>()
            .Entities
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id)
            .Select(candidate => new CoachDto(
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country)))
            .FirstOrDefault();

        return Task.FromResult(coach);
    }
}
