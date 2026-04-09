using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateCoachCommandHandler : IRequestHandler<UpdateCoachCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateCoachCommand> _validator;

    public UpdateCoachCommandHandler(IGymDbContext dbContext, IValidator<UpdateCoachCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var coach = await _dbContext.Coaches
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (coach is null)
        {
            return false;
        }

        coach.FirstName = request.FirstName.Trim();
        coach.LastName = request.LastName.Trim();
        coach.Email = AddressHelper.NormalizeOptional(request.Email);
        coach.Phone = AddressHelper.NormalizeOptional(request.Phone);

        var updatedAddress = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country);
        coach.Address = AddressHelper.Apply(coach.Address, updatedAddress);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
