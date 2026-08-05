using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Points every seat of a sheet the same way — the screen's « Tout présent »,
/// which is how a full class gets pointed in one gesture instead of twenty.
/// Returns false when the id resolves to nothing.
/// <para>
/// It overwrites seats already pointed rather than filling in only the pending
/// ones, because that is what the button says and what the prototype does: a
/// coach who taps it after marking somebody absent by mistake expects the sheet
/// to be uniformly what they just asked for.
/// </para>
/// <para>
/// Waiting-list seats are left alone. The member never got in, so there is
/// nothing to point.
/// </para>
/// </summary>
public sealed class MarkWholeSheetCommand : IRequest<bool>
{
    public MarkWholeSheetCommand(int sessionId, AttendanceStatus status)
    {
        SessionId = sessionId;
        Status = status;
    }

    public int SessionId { get; }

    public AttendanceStatus Status { get; }
}
