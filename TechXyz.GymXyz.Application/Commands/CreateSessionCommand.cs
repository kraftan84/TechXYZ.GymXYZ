using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Books a course into the week. Returns the id of the first occurrence.
/// <para>
/// A recurrence writes one row per date rather than a rule: it is what the grid
/// reads, what occupancy counts and what attendance is marked against. The rows
/// written together share a series id, so editing or cancelling "this one and
/// all the following" stays a single query.
/// </para>
/// </summary>
public sealed class CreateSessionCommand : IRequest<int>
{
    public CreateSessionCommand(
        int courseTemplateId,
        int locationId,
        DateTime startsAt,
        int? coachId = null,
        int? capacity = null,
        int recurrenceWeeks = 1)
    {
        CourseTemplateId = courseTemplateId;
        LocationId = locationId;
        StartsAt = startsAt;
        CoachId = coachId;
        Capacity = capacity;
        RecurrenceWeeks = recurrenceWeeks;
    }

    public int CourseTemplateId { get; }

    public int LocationId { get; }

    public DateTime StartsAt { get; }

    /// <summary>Null for an open slot nobody animates.</summary>
    public int? CoachId { get; }

    /// <summary>
    /// Null takes the course's own capacity, which is the normal case. It is
    /// copied onto every occurrence either way — editing the catalogue later
    /// must not rewrite what already happened.
    /// </summary>
    public int? Capacity { get; }

    /// <summary>One writes a single session; N repeats it weekly, N times.</summary>
    public int RecurrenceWeeks { get; }
}
