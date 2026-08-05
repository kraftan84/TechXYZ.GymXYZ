using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The venue catalogue. <c>GetLocationOptionsQuery</c> answers the same rows cut
/// down to what a picker needs.
/// </summary>
public sealed class GetLocationsQuery : IRequest<LocationsPageDto>;
