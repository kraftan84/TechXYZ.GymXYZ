using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetRoomsPageQuery : IRequest<RoomsPageDto?>;
