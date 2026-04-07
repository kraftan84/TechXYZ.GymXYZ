using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationCommandHandler(IUnitOfWork unitOfWork, IValidator<CreateLocationCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<int> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _unitOfWork.GetDefaultGymAsync(cancellationToken);

        if (defaultGym is null)
        {
            throw new ValidationException("Default gym not found.");
        }

        var location = new Location(request.Name.Trim())
        {
            Address = new Address
            {
                Street = request.Street.Trim(),
                ZipCode = request.ZipCode.Trim(),
                City = request.City.Trim(),
                Country = request.Country.Trim()
            }
        };

        defaultGym.AddLocation(location);
        await _unitOfWork.Save(cancellationToken);

        return location.Id;
    }
}
