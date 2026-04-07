using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateRoomCommand> _validator;

    public UpdateRoomCommandHandler(IGymDbContext dbContext, IValidator<UpdateRoomCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var locations = await _dbContext.Locations
            .Include(location => location.Rooms)
            .Where(location => location.Id == request.LocationId || location.Rooms!.Any(room => room.Id == request.Id))
            .ToListAsync(cancellationToken);

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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
