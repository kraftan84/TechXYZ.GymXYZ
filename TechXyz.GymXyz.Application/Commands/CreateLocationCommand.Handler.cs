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

        var location = new Location(request.Name.Trim())
        {
            Kind = request.Kind,
            TypeLabel = AddressHelper.NormalizeOptional(request.TypeLabel),
            IconKey = AddressHelper.NormalizeOptional(request.IconKey),
            Tone = AddressHelper.NormalizeOptional(request.Tone),
            Capacity = request.Capacity,
            AreaSqm = request.AreaSqm,
            Floor = AddressHelper.NormalizeOptional(request.Floor),
            Note = AddressHelper.NormalizeOptional(request.Note),
            IsOpenAccess = request.IsOpenAccess,
            SiteId = await LocationCompositionHelper.ResolveSiteIdAsync(
                _dbContext, request.SiteId, cancellationToken),
            Address = AddressHelper.BuildOptionalAddress(
                request.Street, request.ZipCode, request.City, request.Country),
            IsWeatherDependent = request.IsWeatherDependent,
            FallbackLocationId = await LocationCompositionHelper.ResolveFallbackLocationIdAsync(
                _dbContext, request.FallbackLocationId, locationId: null, cancellationToken)
        };

        // Written through the navigation rather than the sync helper: the venue
        // has no key yet, and EF fixes the foreign keys up on insert.
        var equipment = OrderedLabelHelper.Normalize(request.Equipment);
        for (var rank = 0; rank < equipment.Count; rank++)
        {
            location.AddEquipment(equipment[rank], rank);
        }

        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}
