using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
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
        var coach = await repository.Entities.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (coach is null)
        {
            return false;
        }

        coach.FirstName = request.FirstName.Trim();
        coach.LastName = request.LastName.Trim();
        coach.Email = AddressHelper.NormalizeOptional(request.Email);
        coach.Phone = AddressHelper.NormalizeOptional(request.Phone);

        var updatedAddress = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country);
        coach.Address = AddressHelper.Apply(coach.Address, updatedAddress);

        await repository.UpdateAsync(coach);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
