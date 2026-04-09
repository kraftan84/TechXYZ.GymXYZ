using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLessonByIdQuery : IRequest<LessonDetailsDto?>
{
    public GetLessonByIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
