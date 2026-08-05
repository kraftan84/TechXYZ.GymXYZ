using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateSiteCommand> _validator;

    public CreateSiteCommandHandler(IGymDbContext dbContext, IValidator<CreateSiteCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _dbContext.GetRequiredDefaultGymAsync(cancellationToken);

        var site = new Site(request.Name.Trim())
        {
            Address = new Address
            {
                Street = request.Street.Trim(),
                ZipCode = request.ZipCode.Trim(),
                City = request.City.Trim(),
                Country = request.Country.Trim()
            }
        };

        defaultGym.AddSite(site);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return site.Id;
    }
}
