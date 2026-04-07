using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateRoomCommand : IRequest<int>
{
    public CreateRoomCommand(string name, int locationId)
    {
        Name = name;
        LocationId = locationId;
    }

    public string Name { get; }
    public int LocationId { get; }
}
