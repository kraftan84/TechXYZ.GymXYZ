using FluentValidation;
using MediatR;
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

        var location = _unitOfWork
            .Repository<Location, int>()
            .Entities
            .FirstOrDefault(candidate => candidate.Id == request.LocationId);

        if (location is null)
        {
            return 0;
        }

        var room = new Room(request.Name.Trim());
        location.AddRoom(room);

        await _unitOfWork.Save(cancellationToken);

        return room.Id;
    }
}
