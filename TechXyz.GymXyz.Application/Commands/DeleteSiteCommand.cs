using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSiteCommand : IRequest<bool>
{
    public DeleteSiteCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
