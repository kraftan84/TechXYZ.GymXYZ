using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateRoomCommand : IRequest<int>
{
    public CreateRoomCommand(string name, int siteId)
    {
        Name = name;
        SiteId = siteId;
    }

    public string Name { get; }
    public int SiteId { get; }
}
