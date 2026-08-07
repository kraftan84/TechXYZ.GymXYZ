using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Buys the next cover on the same plan. A renewal is a new subscription, not an
/// edited one: what the member had last month is history, and stretching its end
/// date would erase it.
/// </summary>
public sealed class RenewSubscriptionCommand : IRequest<int?>, IManagerOnly
{
    public RenewSubscriptionCommand(int subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public int SubscriptionId { get; }
}
