using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Resolves a customer's brand from its slug (the host prefix). Used before
/// authentication too, so the login screen already wears the right colours.
/// </summary>
public sealed record GetTenantBrandQuery(string Slug) : IRequest<TenantBrandDto?>;
