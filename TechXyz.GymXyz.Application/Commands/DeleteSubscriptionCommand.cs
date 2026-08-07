using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSubscriptionCommand : IRequest<bool>, IManagerOnly
{
    public DeleteSubscriptionCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
