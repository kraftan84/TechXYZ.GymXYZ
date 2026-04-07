using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCoachCommand : IRequest<bool>
{
    public DeleteCoachCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
