using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateRoomCommand> _validator;

    public UpdateRoomCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateRoomCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var locationRepository = _unitOfWork.Repository<Location, int>();
        var locations = locationRepository.Entities
            .Include(location => location.Rooms)
            .Where(location => location.Id == request.LocationId || location.Rooms!.Any(room => room.Id == request.Id))
            .ToList();

        var targetLocation = locations.FirstOrDefault(location => location.Id == request.LocationId);
        var currentLocation = locations.FirstOrDefault(location => location.Rooms!.Any(room => room.Id == request.Id));

        if (targetLocation is null || currentLocation is null)
        {
            return false;
        }

        var room = currentLocation.Rooms!.First(candidate => candidate.Id == request.Id);
        room.Name = request.Name.Trim();

        if (currentLocation.Id != targetLocation.Id)
        {
            currentLocation.Rooms!.Remove(room);
            targetLocation.AddRoom(room);
        }

        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
