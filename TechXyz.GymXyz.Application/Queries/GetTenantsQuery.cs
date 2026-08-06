using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Every GymXYZ customer, for the TechXYZ console. Reserved to a
/// <c>PlatformAdmin</c> by the screen's policy — <c>Tenant</c> sits above the
/// tenant filter on purpose, so nothing below stops this from reading them all.
/// </summary>
public sealed record GetTenantsQuery : IRequest<IReadOnlyList<TenantSummaryDto>>;
