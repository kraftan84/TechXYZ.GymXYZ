using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
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

        var defaultGym = await _dbContext.GetRequiredDefaultGymAsync(cancellationToken);

        var location = new Location(request.Name.Trim())
        {
            Address = new Address
            {
                Street = request.Street.Trim(),
                ZipCode = request.ZipCode.Trim(),
                City = request.City.Trim(),
                Country = request.Country.Trim()
            }
        };

        defaultGym.AddLocation(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}
