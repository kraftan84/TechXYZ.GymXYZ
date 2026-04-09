using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSubscriptionCommand : IRequest<bool>
{
    public DeleteSubscriptionCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
