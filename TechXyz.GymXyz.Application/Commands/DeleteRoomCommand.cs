using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteRoomCommand : IRequest<bool>
{
    public DeleteRoomCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
