using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Every GymXYZ customer, for the TechXYZ console. Reserved to a
/// <c>PlatformAdmin</c> by the screen's policy — <c>Tenant</c> sits above the
/// tenant filter on purpose, so nothing below stops this from reading them all.
/// <para>
/// It reads across every customer and it counts through <c>TenantMemberCounter</c>,
/// which lifts the global filter in so many words. It predates
/// <see cref="IPlatformScoped"/> and went without it for two lots — which made
/// the pinned list of border-crossers true only by omission. Named now, before
/// the console adds a dozen more.
/// </para>
/// </summary>
public sealed record GetTenantsQuery
    : IRequest<IReadOnlyList<TenantSummaryDto>>, IPlatformScoped;
