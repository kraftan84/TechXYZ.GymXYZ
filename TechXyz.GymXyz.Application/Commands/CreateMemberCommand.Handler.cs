using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateMemberCommand> _validator;

    public CreateMemberCommandHandler(IUnitOfWork unitOfWork, IValidator<CreateMemberCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<int> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = _unitOfWork
            .Repository<Gym, int>()
            .Entities
            .OrderBy(gym => gym.Id)
            .FirstOrDefault();

        if (defaultGym is null)
        {
            throw new InvalidOperationException("Default gym not found.");
        }

        var member = new Member(request.FirstName.Trim(), request.LastName.Trim())
        {
            Email = Normalize(request.Email),
            Phone = Normalize(request.Phone),
            Address = BuildAddress(request.Street, request.ZipCode, request.City, request.Country)
        };

        defaultGym.AddMember(member);
        await _unitOfWork.Save(cancellationToken);

        return member.Id;
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
