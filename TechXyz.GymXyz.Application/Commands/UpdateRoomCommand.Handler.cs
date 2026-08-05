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

        var sites = await _dbContext.Sites
            .Include(site => site.Rooms)
            .Where(site => site.IsActive && (site.Id == request.SiteId || site.Rooms!.Any(room => room.IsActive && room.Id == request.Id)))
            .ToListAsync(cancellationToken);

        var targetSite = sites.FirstOrDefault(site => site.Id == request.SiteId);
        var currentSite = sites.FirstOrDefault(site => site.Rooms!.Any(room => room.IsActive && room.Id == request.Id));

        if (targetSite is null || currentSite is null)
        {
            return false;
        }

        var room = currentSite.Rooms!.First(candidate => candidate.IsActive && candidate.Id == request.Id);
        room.Name = request.Name.Trim();

        if (currentSite.Id != targetSite.Id)
        {
            currentSite.Rooms!.Remove(room);
            targetSite.AddRoom(room);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
