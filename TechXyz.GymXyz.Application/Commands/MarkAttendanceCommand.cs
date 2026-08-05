using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Points one member on one sheet. Returns false when the id resolves to
/// nothing.
/// <para>
/// One command carries all four verdicts rather than one per verdict. The
/// hand-off asks for a <c>CheckInMemberCommand</c> and a <c>MarkAbsentCommand</c>,
/// but they differ only by the value written — and each would have to enforce
/// the same lock on a closed sheet, which is two places to get it wrong.
/// </para>
/// <para>
/// It is also what the screen does: the segmented control is one tap that sets a
/// value, and tapping "Présent" on somebody already marked absent is a
/// correction, not a different operation.
/// </para>
/// </summary>
public sealed class MarkAttendanceCommand : IRequest<bool>
{
    public MarkAttendanceCommand(int registrationId, AttendanceStatus status)
    {
        RegistrationId = registrationId;
        Status = status;
    }

    public int RegistrationId { get; }

    public AttendanceStatus Status { get; }
}
