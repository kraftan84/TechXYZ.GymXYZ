using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

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
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);
        if (location is null)
        {
            return false;
        }

        // Whoever pointed at this venue stops doing so. Both references are
        // optional, so releasing them costs nothing and leaving them would let
        // an archived venue keep showing up on a course record and on the
        // "repli" line of an outdoor one.
        var templates = await _dbContext.CourseTemplates
            .Where(template => template.DefaultLocationId == location.Id)
            .ToListAsync(cancellationToken);
        foreach (var template in templates)
        {
            template.DefaultLocationId = null;
        }

        var dependents = await _dbContext.Locations
            .Where(candidate => candidate.FallbackLocationId == location.Id)
            .ToListAsync(cancellationToken);
        foreach (var dependent in dependents)
        {
            dependent.FallbackLocationId = null;
        }

        location.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
