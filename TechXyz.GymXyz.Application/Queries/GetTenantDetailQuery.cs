using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Everything the console's two panels show about one customer. Reserved to a
/// <c>PlatformAdmin</c> by the screen's policy: <c>Tenant</c> and
/// <c>Invoice</c> both sit above the tenant filter, so nothing below stops this
/// from reading a customer the request is not being served as.
/// <para>
/// Marked <see cref="IPlatformScoped"/> for the same reason as
/// <see cref="GetTenantsQuery"/>: it reads a customer nobody is being served as,
/// and it must work with no ambient tenant at all.
/// </para>
/// </summary>
public sealed record GetTenantDetailQuery(int TenantId)
    : IRequest<TenantDetailDto?>, IPlatformScoped;
