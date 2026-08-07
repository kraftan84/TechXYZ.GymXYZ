using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateGymIdentityCommandHandler : IRequestHandler<UpdateGymIdentityCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<UpdateGymIdentityCommand> _validator;

    public UpdateGymIdentityCommandHandler(
        IGymDbContext dbContext,
        ITenantContext tenantContext,
        IValidator<UpdateGymIdentityCommand> validator)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateGymIdentityCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(
                candidate => candidate.Id == _tenantContext.Current && candidate.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            return false;
        }

        tenant.DisplayName = request.Name;
        tenant.Baseline = request.Baseline;
        tenant.Capacity = request.Capacity;
        tenant.Siret = request.Siret;
        tenant.Email = request.Email;
        tenant.Phone = request.Phone;
        tenant.AreaLabel = request.AreaLabel;
        tenant.ShowSchoolVacations = request.ShowSchoolVacations;

        if (request.AreaLabel is not null)
        {
            // A coach who works around Thonon has no premises. Keeping a street
            // beside the zone would let a stale address outlive the move and
            // reappear on an invoice.
            tenant.Street = null;
            tenant.ZipCode = null;
            tenant.City = null;

            // The school zone is read off the postcode, so without one there is
            // no calendar to follow. Cleared with the address for the same
            // reason it is: a setting left on would outlive what made it
            // meaningful, and the planning would mark a zone nobody chose.
            tenant.ShowSchoolVacations = false;
        }
        else
        {
            tenant.Street = request.Street;
            tenant.ZipCode = request.ZipCode;
            tenant.City = request.City;
        }

        var settings = await _dbContext.GymSettings
            .Include(candidate => candidate.OpeningHours)
            .FirstOrDefaultAsync(candidate => candidate.IsActive, cancellationToken);

        if (settings is null)
        {
            settings = new GymSettings();
            _dbContext.GymSettings.Add(settings);
        }

        // Cached, not chosen: the zone follows the postcode, and a gym that moves
        // département must not keep signalling the holidays of the old one.
        settings.SchoolZone = tenant.ZipCode is null ? null : SchoolZones.ForPostcode(tenant.ZipCode);

        ApplyOpeningHours(settings, request.OpeningHours);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// The panel submits the whole list, so a line the gym removed is a line
    /// that is no longer in it. Removals are soft, as everywhere else.
    /// </summary>
    private static void ApplyOpeningHours(GymSettings settings, IReadOnlyList<OpeningHoursInput> submitted)
    {
        settings.OpeningHours ??= new List<OpeningHours>();

        var submittedIds = submitted.Where(line => line.Id > 0).Select(line => line.Id).ToHashSet();

        foreach (var existing in settings.OpeningHours.Where(hours => hours.IsActive))
        {
            if (!submittedIds.Contains(existing.Id))
            {
                existing.IsActive = false;
            }
        }

        for (var rank = 0; rank < submitted.Count; rank++)
        {
            var line = submitted[rank];
            var target = line.Id > 0
                ? settings.OpeningHours.FirstOrDefault(hours => hours.Id == line.Id)
                : null;

            if (target is null)
            {
                target = new OpeningHours { Settings = settings };
                settings.OpeningHours.Add(target);
            }

            target.IsActive = true;
            target.DayFrom = line.DayFrom;
            target.DayTo = line.DayTo;
            target.OpensAt = line.OpensAt;
            target.ClosesAt = line.ClosesAt;
            target.Rank = rank;
        }
    }
}
