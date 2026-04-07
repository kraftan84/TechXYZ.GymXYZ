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

        var location = await _dbContext.Locations.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (location is null)
        {
            return false;
        }

        location.Name = request.Name.Trim();
        location.Address ??= new Address();
        location.Address.Street = request.Street.Trim();
        location.Address.ZipCode = request.ZipCode.Trim();
        location.Address.City = request.City.Trim();
        location.Address.Country = request.Country.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
