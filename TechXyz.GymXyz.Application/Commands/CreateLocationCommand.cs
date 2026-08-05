using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommand : IRequest<int>
{
    public CreateLocationCommand(string name, int siteId)
    {
        Name = name;
        SiteId = siteId;
    }

    public string Name { get; }
    public int SiteId { get; }
}
