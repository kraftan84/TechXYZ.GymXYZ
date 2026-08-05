using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateLocationCommand> _validator;

    public UpdateLocationCommandHandler(IGymDbContext dbContext, IValidator<UpdateLocationCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var sites = await _dbContext.Sites
            .Include(site => site.Locations)
            .Where(site => site.IsActive && (site.Id == request.SiteId || site.Locations!.Any(location => location.IsActive && location.Id == request.Id)))
            .ToListAsync(cancellationToken);

        var targetSite = sites.FirstOrDefault(site => site.Id == request.SiteId);
        var currentSite = sites.FirstOrDefault(site => site.Locations!.Any(location => location.IsActive && location.Id == request.Id));

        if (targetSite is null || currentSite is null)
        {
            return false;
        }

        var location = currentSite.Locations!.First(candidate => candidate.IsActive && candidate.Id == request.Id);
        location.Name = request.Name.Trim();

        if (currentSite.Id != targetSite.Id)
        {
            currentSite.Locations!.Remove(location);
            targetSite.AddLocation(location);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
