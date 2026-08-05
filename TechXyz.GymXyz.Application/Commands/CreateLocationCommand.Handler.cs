using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationCommandHandler(IGymDbContext dbContext, IValidator<CreateLocationCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _dbContext.Sites
            .FirstOrDefaultAsync(candidate => candidate.Id == request.SiteId && candidate.IsActive, cancellationToken);

        if (site is null)
        {
            throw new ValidationException("Site not found.");
        }

        var location = new Location(request.Name.Trim());
        site.AddLocation(location);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}
