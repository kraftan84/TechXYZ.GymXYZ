using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Closes an account without destroying it.
/// <para>
/// The person keeps their history — a revoked coach still signed the attendance
/// sheets they signed, and a revoked member still holds the subscriptions they
/// bought. Only the door closes.
/// </para>
/// </summary>
public sealed class RevokeAccessCommand : IRequest<bool>, IManagerOnly
{
    public RevokeAccessCommand(string userId)
    {
        UserId = userId;
    }

    public string UserId { get; }
}
