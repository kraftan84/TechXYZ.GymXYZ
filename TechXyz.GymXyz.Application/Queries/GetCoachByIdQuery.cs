using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachByIdQuery : IRequest<CoachDto?>
{
    public GetCoachByIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
