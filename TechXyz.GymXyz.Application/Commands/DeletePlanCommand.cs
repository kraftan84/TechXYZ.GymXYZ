using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Takes a formule off sale.
/// <para>
/// A soft delete, and the distinction matters here more than anywhere: the
/// members already on the plan keep it. Their subscription carries its own
/// price, its own entry count and its own end date, so nothing they bought
/// changes — the plan simply stops being offered, disappears from the cards and
/// from the picker, and <c>RenewSubscriptionCommand</c> refuses to put anybody
/// back on it.
/// </para>
/// </summary>
public sealed class DeletePlanCommand : IRequest<bool>, IManagerOnly
{
    public DeletePlanCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
