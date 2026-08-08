using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class CheckSubdomainAvailabilityQueryHandler
    : IRequestHandler<CheckSubdomainAvailabilityQuery, SubdomainAvailabilityDto>
{
    private readonly IGymDbContext _dbContext;

    public CheckSubdomainAvailabilityQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubdomainAvailabilityDto> Handle(
        CheckSubdomainAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var candidate = Subdomains.Normalise(request.Candidate);

        if (candidate.Length == 0)
        {
            return new SubdomainAvailabilityDto(
                candidate,
                false,
                "Lettres, chiffres et tirets. Un nom de domaine à vous est possible plus tard.");
        }

        if (!Subdomains.IsWellFormed(candidate))
        {
            return new SubdomainAvailabilityDto(
                candidate,
                false,
                $"Entre {Subdomains.MinimumLength} et {Subdomains.MaximumLength} caractères, "
                + "sans tiret au début ni à la fin.");
        }

        if (Subdomains.IsReserved(candidate))
        {
            // Said plainly rather than as "already taken": it is not taken, it is
            // not on offer, and a suggestion here would be a suggestion to keep
            // trying names that will also be refused.
            return new SubdomainAvailabilityDto(
                candidate,
                false,
                "Cette adresse est réservée par GymXYZ. Choisissez-en une autre.");
        }

        // Two ways to be taken: a customer already answers on it, or somebody
        // else asked for it first and has not been answered yet. Missing the
        // second would let two requests through and turn the collision into the
        // console's problem, at provisioning, in front of a customer.
        var taken = await IsTakenAsync(candidate, cancellationToken);

        if (!taken)
        {
            return new SubdomainAvailabilityDto(
                candidate,
                true,
                $"{candidate}.gymxyz.fr est disponible.");
        }

        var known = await KnownNamesAsync(candidate, cancellationToken);

        return new SubdomainAvailabilityDto(
            candidate,
            false,
            $"{candidate}.gymxyz.fr est déjà pris.",
            Subdomains.Suggest(candidate, proposal => known.Contains(proposal)));
    }

    private async Task<bool> IsTakenAsync(string candidate, CancellationToken cancellationToken)
    {
        var byCustomer = await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(tenant => tenant.Slug == candidate, cancellationToken);

        if (byCustomer)
        {
            return true;
        }

        return await _dbContext.SpaceRequests
            .AsNoTracking()
            .AnyAsync(
                other => other.RequestedSubdomain == candidate
                         && other.Status != SpaceRequestStatus.Refused,
                cancellationToken);
    }

    /// <summary>
    /// Everything starting with the stem, in one pass — so proposing an
    /// alternative does not cost one round trip per attempt.
    /// <para>
    /// <c>LIKE</c> rather than <c>StartsWith</c>: the MySQL provider translates a
    /// parameterised <c>StartsWith</c> into a <c>COLLATE</c> it then cannot assign
    /// a type mapping to, and the query throws at translation time. The in-memory
    /// provider the handler tests use runs it happily, so this only ever appears
    /// against a real database. A stem is normalised to letters, digits and
    /// dashes, so it can carry no <c>LIKE</c> wildcard of its own.
    /// </para>
    /// </summary>
    private async Task<HashSet<string>> KnownNamesAsync(string stem, CancellationToken cancellationToken)
    {
        var slugs = await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => EF.Functions.Like(tenant.Slug, stem + "%"))
            .Select(tenant => tenant.Slug)
            .ToListAsync(cancellationToken);

        var asked = await _dbContext.SpaceRequests
            .AsNoTracking()
            .Where(other => EF.Functions.Like(other.RequestedSubdomain, stem + "%")
                            && other.Status != SpaceRequestStatus.Refused)
            .Select(other => other.RequestedSubdomain)
            .ToListAsync(cancellationToken);

        return [.. slugs, .. asked];
    }
}
