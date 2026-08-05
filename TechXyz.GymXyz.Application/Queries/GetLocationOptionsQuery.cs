using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The flat list of venues, for the pickers. <c>GetLocationsQuery</c> answers
/// the same rows with everything a card draws, which is more than a picker
/// needs.
/// </summary>
public sealed class GetLocationOptionsQuery : IRequest<List<LocationOptionDto>>;
