using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteLocationCommand> _validator;

    public DeleteLocationCommandHandler(IGymDbContext dbContext, IValidator<DeleteLocationCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var location = await _dbContext.Locations
            .Include(candidate => candidate.Rooms)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

        if (location is null)
        {
            return false;
        }

        foreach (var room in location.Rooms?.ToList() ?? [])
        {
            _dbContext.Rooms.Remove(room);
        }

        _dbContext.Locations.Remove(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
