using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>The flat list of buildings, for the venue drawer's site picker.</summary>
public sealed class GetSiteOptionsQuery : IRequest<List<SiteOptionDto>>;
