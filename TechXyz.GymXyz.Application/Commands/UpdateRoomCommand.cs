using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateRoomCommand : IRequest<bool>
{
    public UpdateRoomCommand(int id, string name, int locationId)
    {
        Id = id;
        Name = name;
        LocationId = locationId;
    }

    public int Id { get; }
    public string Name { get; }
    public int LocationId { get; }
}
