using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCoachCommand : IRequest<bool>, IManagerOnly
{
    public DeleteCoachCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
