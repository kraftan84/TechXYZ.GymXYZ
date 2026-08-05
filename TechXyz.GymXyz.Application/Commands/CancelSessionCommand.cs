using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Calls a session off. Returns false when the id resolves to nothing.
/// <para>
/// The row stays and takes the <c>Cancelled</c> status rather than being
/// removed: the members registered have to keep seeing why, and the slot has to
/// stop counting towards occupancy without disappearing from the history.
/// Warning them is a lot 8 matter, once there is a channel to warn them through.
/// </para>
/// </summary>
public sealed class CancelSessionCommand : IRequest<bool>
{
    public CancelSessionCommand(int id, string? reason = null, SessionEditScope scope = SessionEditScope.ThisOne)
    {
        Id = id;
        Reason = reason;
        Scope = scope;
    }

    public int Id { get; }

    /// <summary>Shown to the members who had a seat.</summary>
    public string? Reason { get; }

    public SessionEditScope Scope { get; }
}
