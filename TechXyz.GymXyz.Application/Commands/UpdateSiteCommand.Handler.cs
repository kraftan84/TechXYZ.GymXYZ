using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateSiteCommand> _validator;

    public UpdateSiteCommandHandler(IGymDbContext dbContext, IValidator<UpdateSiteCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _dbContext.Sites
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (site is null)
        {
            return false;
        }

        site.Name = request.Name.Trim();
        site.Address ??= new Address();
        site.Address.Street = request.Street.Trim();
        site.Address.ZipCode = request.ZipCode.Trim();
        site.Address.City = request.City.Trim();
        site.Address.Country = request.Country.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
