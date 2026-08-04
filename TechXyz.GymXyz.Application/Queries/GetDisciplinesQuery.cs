using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>The disciplines a coach or a course can be attached to.</summary>
public sealed class GetDisciplinesQuery : IRequest<List<DisciplineDto>>;
