using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Whether <c>{candidate}.gymxyz.fr</c> can be had. Public — it answers a field
/// on the open onboarding form, before anybody has an account.
/// <para>
/// <see cref="IPlatformScoped"/> because both things it consults sit outside
/// every tenant: the customers themselves, and the requests already asking for a
/// name. Under a tenant filter it would see neither and cheerfully hand the same
/// address to two people.
/// </para>
/// </summary>
public sealed class CheckSubdomainAvailabilityQuery : IRequest<SubdomainAvailabilityDto>, IPlatformScoped
{
    public CheckSubdomainAvailabilityQuery(string? candidate)
    {
        Candidate = candidate;
    }

    /// <summary>As typed. The handler normalises — the caller should not have to.</summary>
    public string? Candidate { get; }
}
