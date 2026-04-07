using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateLocationCommand> _validator;

    public UpdateLocationCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateLocationCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Location, int>();
        var location = await repository.Entities.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (location is null)
        {
            return false;
        }

        location.Name = request.Name.Trim();
        location.Address ??= new Address();
        location.Address.Street = request.Street.Trim();
        location.Address.ZipCode = request.ZipCode.Trim();
        location.Address.City = request.City.Trim();
        location.Address.Country = request.Country.Trim();

        await repository.UpdateAsync(location);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
