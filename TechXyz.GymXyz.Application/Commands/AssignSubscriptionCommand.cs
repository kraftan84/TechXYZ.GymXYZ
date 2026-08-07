using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Sells a plan to a member. Everything the subscription will carry — how long
/// it runs, how many entries it holds, what it costs — comes off the plan; the
/// caller chooses the member, the plan and the day it starts.
/// </summary>
public sealed class AssignSubscriptionCommand : IRequest<int>, IManagerOnly
{
    public AssignSubscriptionCommand(int memberId, int planId, DateOnly? startedOn, bool autoRenew)
    {
        MemberId = memberId;
        PlanId = planId;
        StartedOn = startedOn ?? DateOnly.FromDateTime(DateTime.Today);
        AutoRenew = autoRenew;
    }

    public int MemberId { get; }
    public int PlanId { get; }
    public DateOnly StartedOn { get; }
    public bool AutoRenew { get; }
}
