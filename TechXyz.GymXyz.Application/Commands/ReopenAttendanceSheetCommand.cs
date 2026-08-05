using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Unlocks a validated sheet so it can be corrected. Returns false when the id
/// resolves to nothing.
/// <para>
/// Reserved to a <c>GymManager</c>, and traced: the session records who reopened
/// it and when. This is the one way a record that was already closed gets
/// rewritten, so it does not happen anonymously — which is what
/// <c>01-LOTS.md</c> asks for when it leaves the decision open.
/// </para>
/// <para>
/// The check is in the handler rather than on the button. Hiding the control
/// from a coach is courtesy; the handler is the only place a caller cannot go
/// around.
/// </para>
/// </summary>
public sealed class ReopenAttendanceSheetCommand : IRequest<bool>
{
    public ReopenAttendanceSheetCommand(int sessionId)
    {
        SessionId = sessionId;
    }

    public int SessionId { get; }
}
