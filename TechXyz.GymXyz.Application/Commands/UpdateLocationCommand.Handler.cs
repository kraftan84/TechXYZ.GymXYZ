using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

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

        var location = await _dbContext.Locations
            .Include(candidate => candidate.Address)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);
        if (location is null)
        {
            return false;
        }

        location.Name = request.Name.Trim();
        location.Kind = request.Kind;
        location.TypeLabel = AddressHelper.NormalizeOptional(request.TypeLabel);
        location.IconKey = AddressHelper.NormalizeOptional(request.IconKey);
        location.Tone = AddressHelper.NormalizeOptional(request.Tone);
        location.Capacity = request.Capacity;
        location.AreaSqm = request.AreaSqm;
        location.Floor = AddressHelper.NormalizeOptional(request.Floor);
        location.Note = AddressHelper.NormalizeOptional(request.Note);
        location.IsOpenAccess = request.IsOpenAccess;
        location.IsWeatherDependent = request.IsWeatherDependent;

        location.SiteId = await LocationCompositionHelper.ResolveSiteIdAsync(
            _dbContext, request.SiteId, cancellationToken);

        location.FallbackLocationId = await LocationCompositionHelper.ResolveFallbackLocationIdAsync(
            _dbContext, request.FallbackLocationId, location.Id, cancellationToken);

        location.Address = AddressHelper.Apply(
            location.Address,
            AddressHelper.BuildOptionalAddress(
                request.Street, request.ZipCode, request.City, request.Country));

        await LocationCompositionHelper.SyncEquipmentAsync(
            _dbContext, location, request.Equipment ?? [], cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
