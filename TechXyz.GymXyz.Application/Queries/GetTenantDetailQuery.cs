using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Everything the console's two panels show about one customer. Reserved to a
/// <c>PlatformAdmin</c> by the screen's policy: <c>Tenant</c> and
/// <c>Invoice</c> both sit above the tenant filter, so nothing below stops this
/// from reading a customer the request is not being served as.
/// </summary>
public sealed record GetTenantDetailQuery(int TenantId) : IRequest<TenantDetailDto?>;
