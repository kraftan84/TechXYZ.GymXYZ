using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateCoachCommandHandler : IRequestHandler<UpdateCoachCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCoachCommand> _validator;

    public UpdateCoachCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateCoachCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Coach, int>();
        var coach = repository.Entities.FirstOrDefault(candidate => candidate.Id == request.Id);
        if (coach is null)
        {
            return false;
        }

        coach.FirstName = request.FirstName.Trim();
        coach.LastName = request.LastName.Trim();
        coach.Email = Normalize(request.Email);
        coach.Phone = Normalize(request.Phone);

        var updatedAddress = BuildAddress(request.Street, request.ZipCode, request.City, request.Country);
        if (updatedAddress is null)
        {
            coach.Address = null;
        }
        else if (coach.Address is null)
        {
            coach.Address = updatedAddress;
        }
        else
        {
            coach.Address.Street = updatedAddress.Street;
            coach.Address.ZipCode = updatedAddress.ZipCode;
            coach.Address.City = updatedAddress.City;
            coach.Address.Country = updatedAddress.Country;
        }

        await repository.UpdateAsync(coach);
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
