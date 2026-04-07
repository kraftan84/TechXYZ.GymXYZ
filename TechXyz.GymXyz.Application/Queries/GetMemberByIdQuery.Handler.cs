using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMemberByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<MemberDto?> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var member = _unitOfWork
            .Repository<Member, int>()
            .Entities
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id)
            .Select(candidate => new MemberDto(
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.Subscriptions!.Any(subscription =>
                    subscription.StartDate <= today && subscription.EndDate >= today),
                candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country)))
            .FirstOrDefault();

        return Task.FromResult(member);
    }
}
