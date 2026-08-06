using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Calls a session off. Returns false when the id resolves to nothing.
/// <para>
/// The row stays and takes the <c>Cancelled</c> status rather than being
/// removed: the members registered have to keep seeing why, and the slot has to
/// stop counting towards occupancy without disappearing from the history.
/// </para>
/// <para>
/// Everybody holding a seat is now told, subject to the gym's « Annulation de
/// cours » switch. The cancellation is committed first and stands whatever the
/// sending does — a session that is off must not come back on because a mail
/// server was unreachable, and that is the one moment a gym cannot afford the
/// screen to refuse.
/// </para>
/// </summary>
public sealed class CancelSessionCommand : IRequest<NotificationOutcomeDto>
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
