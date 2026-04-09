using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateCoachCommand> _validator;

    public CreateCoachCommandHandler(IGymDbContext dbContext, IValidator<CreateCoachCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _dbContext.GetRequiredDefaultGymAsync(cancellationToken);

        var coach = new Coach(request.FirstName.Trim(), request.LastName.Trim())
        {
            Email = AddressHelper.NormalizeOptional(request.Email),
            Phone = AddressHelper.NormalizeOptional(request.Phone),
            Address = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country)
        };

        defaultGym.AddCoach(coach);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return coach.Id;
    }
}
