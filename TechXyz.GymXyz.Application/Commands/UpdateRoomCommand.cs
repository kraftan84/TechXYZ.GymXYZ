using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateRoomCommand : IRequest<bool>
{
    public UpdateRoomCommand(int id, string name, int siteId)
    {
        Id = id;
        Name = name;
        SiteId = siteId;
    }

    public int Id { get; }
    public string Name { get; }
    public int SiteId { get; }
}
