using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed record GetTenantBrandByIdQuery(int TenantId) : IRequest<TenantBrandDto?>;
