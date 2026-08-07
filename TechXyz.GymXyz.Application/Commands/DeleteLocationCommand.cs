using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLocationCommand : IRequest<bool>, IManagerOnly
{
    public DeleteLocationCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
