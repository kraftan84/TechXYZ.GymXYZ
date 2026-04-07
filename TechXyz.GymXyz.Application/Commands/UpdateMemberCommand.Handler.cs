using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateMemberCommand> _validator;

    public UpdateMemberCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateMemberCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Member, int>();
        var member = repository.Entities.FirstOrDefault(candidate => candidate.Id == request.Id);
        if (member is null)
        {
            return false;
        }

        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.Email = Normalize(request.Email);
        member.Phone = Normalize(request.Phone);

        var updatedAddress = BuildAddress(request.Street, request.ZipCode, request.City, request.Country);
        if (updatedAddress is null)
        {
            member.Address = null;
        }
        else if (member.Address is null)
        {
            member.Address = updatedAddress;
        }
        else
        {
            member.Address.Street = updatedAddress.Street;
            member.Address.ZipCode = updatedAddress.ZipCode;
            member.Address.City = updatedAddress.City;
            member.Address.Country = updatedAddress.Country;
        }

        await repository.UpdateAsync(member);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Address? BuildAddress(string? street, string? zipCode, string? city, string? country)
    {
        var normalizedStreet = Normalize(street);
        var normalizedZipCode = Normalize(zipCode);
        var normalizedCity = Normalize(city);
        var normalizedCountry = Normalize(country);

        if (normalizedStreet is null && normalizedZipCode is null && normalizedCity is null && normalizedCountry is null)
        {
            return null;
        }

        return new Address
        {
            Street = normalizedStreet ?? string.Empty,
            ZipCode = normalizedZipCode ?? string.Empty,
            City = normalizedCity ?? string.Empty,
            Country = normalizedCountry ?? string.Empty
        };
    }
}
