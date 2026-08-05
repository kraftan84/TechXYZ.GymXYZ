using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLocationCommand : IRequest<bool>
{
    public DeleteLocationCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
