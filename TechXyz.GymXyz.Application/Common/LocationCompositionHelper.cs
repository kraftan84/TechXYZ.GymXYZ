using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What a venue is made of beyond its own columns: the building it sits in, the
/// venue it falls back to, and the equipment it holds. Create and update write
/// all three the same way, so the rules live here once.
/// </summary>
public static class LocationCompositionHelper
{
    /// <summary>
    /// The building, when one was named. An id that resolves to nothing active
    /// is a broken request, not a silent omission.
    /// </summary>
    public static async Task<int?> ResolveSiteIdAsync(
        IGymDbContext dbContext,
        int? siteId,
        CancellationToken cancellationToken)
    {
        if (siteId is not { } id)
        {
            return null;
        }

        var exists = await dbContext.Sites
            .AnyAsync(site => site.Id == id && site.IsActive, cancellationToken);

        return exists ? id : throw new ValidationException("Site introuvable.");
    }

    /// <summary>
    /// The venue to fall back to. It has to be a studio — falling back from one
    /// park to another shelters nobody — and it cannot be the venue itself.
    /// </summary>
    public static async Task<int?> ResolveFallbackLocationIdAsync(
        IGymDbContext dbContext,
        int? fallbackLocationId,
        int? locationId,
        CancellationToken cancellationToken)
    {
        if (fallbackLocationId is not { } id)
        {
            return null;
        }

        if (locationId is { } current && current == id)
        {
            throw new ValidationException(LocationRules.FallbackSelfMessage);
        }

        var fallback = await dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.IsActive, cancellationToken);

        if (fallback is null)
        {
            throw new ValidationException("Lieu de repli introuvable.");
        }

        if (fallback.Kind != LocationKind.Studio)
        {
            throw new ValidationException(LocationRules.FallbackKindMessage);
        }

        return id;
    }

    /// <summary>
    /// Replaces the equipment list, in the order given — that is the order the
    /// chips are drawn in, and the first four are the ones the card shows.
    /// </summary>
    public static async Task SyncEquipmentAsync(
        IGymDbContext dbContext,
        Location location,
        IReadOnlyList<string> equipment,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.LocationEquipment
            .Where(candidate => candidate.LocationId == location.Id)
            .ToListAsync(cancellationToken);

        OrderedLabelHelper.Replace(
            dbContext.LocationEquipment,
            existing,
            OrderedLabelHelper.Normalize(equipment),
            (label, rank) => new LocationEquipment(label)
            {
                LocationId = location.Id,
                Rank = rank,
                TenantId = location.TenantId
            });
    }
}
