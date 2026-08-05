using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Validates the sheet — the screen's « Valider la feuille » — and locks it.
/// Returns false when the id resolves to nothing.
/// <para>
/// Seats still pending are left pending rather than being read as absences. The
/// prototype lets a coach validate a partly-pointed sheet, and turning silence
/// into a verdict would put no-shows on members nobody looked at.
/// </para>
/// </summary>
public sealed class CloseAttendanceSheetCommand : IRequest<bool>
{
    public CloseAttendanceSheetCommand(int sessionId)
    {
        SessionId = sessionId;
    }

    public int SessionId { get; }
}
