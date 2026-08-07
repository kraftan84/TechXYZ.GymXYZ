using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSiteCommand : IRequest<bool>, IManagerOnly
{
    public DeleteSiteCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
