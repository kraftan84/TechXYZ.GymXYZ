using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSiteCommandHandler : IRequestHandler<DeleteSiteCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteSiteCommand> _validator;

    public DeleteSiteCommandHandler(IGymDbContext dbContext, IValidator<DeleteSiteCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteSiteCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _dbContext.Sites
            .Include(candidate => candidate.Rooms)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);

        if (site is null)
        {
            return false;
        }

        foreach (var room in site.Rooms?.ToList() ?? [])
        {
            room.IsActive = false;
        }

        site.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
