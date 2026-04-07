using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateRoomCommand> _validator;

    public CreateRoomCommandHandler(IUnitOfWork unitOfWork, IValidator<CreateRoomCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<int> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var location = await _unitOfWork
            .Repository<Location, int>()
            .Entities
            .FirstOrDefaultAsync(candidate => candidate.Id == request.LocationId, cancellationToken);

        if (location is null)
        {
            throw new ValidationException("Location not found.");
        }

        var room = new Room(request.Name.Trim());
        location.AddRoom(room);

        await _unitOfWork.Save(cancellationToken);

        return room.Id;
    }
}
