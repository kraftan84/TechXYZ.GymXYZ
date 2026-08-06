using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Asks members who have stopped turning up whether all is well — the
/// « Relancer » of the absentees card, and « Relancer tout le monde » with the
/// whole list.
/// <para>
/// One command for one member or for several, because the card offers both and
/// the difference is a loop. Sending to a list is not a batch job: a gym chases
/// a handful of people, and the screen waits for the answer rather than
/// promising one later.
/// </para>
/// <para>
/// <b>Not gated on a notification switch</b>, unlike the other two sends of this
/// lot. The six switches describe messages GymXYZ sends <em>by itself</em> —
/// "au membre, 7 jours avant", "dès qu'un prélèvement est rejeté". This one has
/// no automatic counterpart: somebody looked at the card and clicked. A
/// preference about automation is not an answer to a person, and inventing a
/// seventh switch would put a control in the panel for a message nothing sends
/// on its own.
/// </para>
/// </summary>
public sealed class SendAbsenceReminderCommand : IRequest<NotificationOutcomeDto>
{
    public SendAbsenceReminderCommand(IReadOnlyList<int> memberIds)
    {
        MemberIds = memberIds.Distinct().ToList();
    }

    public SendAbsenceReminderCommand(int memberId)
        : this([memberId])
    {
    }

    public IReadOnlyList<int> MemberIds { get; }
}
