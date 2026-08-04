using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The flat list of studios, for the pickers. <c>GetRoomsPageQuery</c> answers
/// the same rows grouped by location, which is more than a picker needs.
/// </summary>
public sealed class GetRoomsQuery : IRequest<List<RoomDto>>;
