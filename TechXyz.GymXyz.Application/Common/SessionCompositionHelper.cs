using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The three invariants a session has to satisfy against the rest of the
/// planning, and the lookups they need. They live here rather than in the
/// validators because every one of them is a question for the database, and
/// creating and editing must ask it the same way.
/// <para>
/// A coach marked away is deliberately not one of them: the hand-off calls that
/// an alert, not a block, and the drawer raises it from the leave date it
/// already knows. Refusing the write would stop a manager covering a gap.
/// </para>
/// </summary>
public static class SessionCompositionHelper
{
    /// <summary>
    /// Refuses the write with a message the user actually reads.
    /// <para>
    /// <c>new ValidationException(text)</c> fills the exception's own message but
    /// leaves <c>Errors</c> empty, and the toast is built from <c>Errors</c> — so
    /// a plain-text throw reaches the screen as "Validation invalide". These
    /// invariants exist to say which room is taken and by whom, so they are
    /// raised as a failure, not as a message.
    /// </para>
    /// </summary>
    private static ValidationException Refuse(string field, string message) =>
        new([new ValidationFailure(field, message)]);

    public static async Task<CourseTemplate> LoadCourseTemplateAsync(
        IGymDbContext dbContext,
        int courseTemplateId,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.CourseTemplates
            .FirstOrDefaultAsync(
                candidate => candidate.Id == courseTemplateId && candidate.IsActive,
                cancellationToken);

        return template ?? throw Refuse(SessionFieldNames.Course, "Cours introuvable.");
    }

    public static async Task<Location> LoadLocationAsync(
        IGymDbContext dbContext,
        int locationId,
        CancellationToken cancellationToken)
    {
        var location = await dbContext.Locations
            .FirstOrDefaultAsync(
                candidate => candidate.Id == locationId && candidate.IsActive,
                cancellationToken);

        return location ?? throw Refuse(SessionFieldNames.Location, "Lieu introuvable.");
    }

    public static async Task<Coach?> LoadCoachAsync(
        IGymDbContext dbContext,
        int? coachId,
        CancellationToken cancellationToken)
    {
        if (coachId is not { } id)
        {
            return null;
        }

        var coach = await dbContext.Coaches
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.IsActive, cancellationToken);

        return coach ?? throw Refuse(SessionFieldNames.Coach, "Coach introuvable.");
    }

    /// <summary>
    /// Invariant 1 — a session never seats more than the venue holds. The venue
    /// with a capacity of one is the member's home, which is what keeps a
    /// private lesson private.
    /// </summary>
    public static void EnsureFitsInLocation(Location location, int capacity)
    {
        if (capacity > location.Capacity)
        {
            throw Refuse(
                SessionFieldNames.Capacity,
                SessionRules.OverCapacityMessage(location.Name, location.Capacity));
        }
    }

    /// <summary>
    /// Invariants 2 and 3 — the venue is free and the coach is free, for every
    /// occurrence about to be written. A cancelled session frees its slot.
    /// <paramref name="ignoredSessionIds"/> lets an edit ignore the rows it is
    /// itself rewriting.
    /// </summary>
    public static async Task EnsureSlotsAreFreeAsync(
        IGymDbContext dbContext,
        IReadOnlyList<(DateTime StartsAt, DateTime EndsAt)> slots,
        int locationId,
        int? coachId,
        IReadOnlyCollection<int> ignoredSessionIds,
        CancellationToken cancellationToken)
    {
        if (slots.Count == 0)
        {
            return;
        }

        var from = slots.Min(slot => slot.StartsAt);
        var to = slots.Max(slot => slot.EndsAt);

        // One round trip for the whole series: the window is bounded, and asking
        // per occurrence would be a query per week.
        var neighbours = await dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                session.IsActive &&
                session.Status != SessionStatus.Cancelled &&
                !ignoredSessionIds.Contains(session.Id) &&
                session.EndsAt > from &&
                session.StartsAt < to &&
                (session.LocationId == locationId ||
                 (coachId != null && session.CoachId == coachId)))
            .Select(session => new
            {
                session.StartsAt,
                session.EndsAt,
                session.LocationId,
                session.CoachId,
                LocationName = session.Location!.Name,
                CoachFirstName = session.Coach == null ? null : session.Coach.FirstName,
                CoachLastName = session.Coach == null ? null : session.Coach.LastName
            })
            .ToListAsync(cancellationToken);

        if (neighbours.Count == 0)
        {
            return;
        }

        foreach (var slot in slots)
        {
            // Touching at the boundary is not overlapping: a class ending at
            // 18:00 and the next starting at 18:00 share the room for no time.
            var clashes = neighbours
                .Where(other => other.StartsAt < slot.EndsAt && other.EndsAt > slot.StartsAt)
                .ToList();

            var when = WhenLabel(slot.StartsAt);

            var busyLocation = clashes.FirstOrDefault(other => other.LocationId == locationId);
            if (busyLocation is not null)
            {
                throw Refuse(
                    SessionFieldNames.Location,
                    SessionRules.LocationBusyMessage(busyLocation.LocationName, when));
            }

            var busyCoach = coachId is null
                ? null
                : clashes.FirstOrDefault(other => other.CoachId == coachId);
            if (busyCoach is not null)
            {
                throw Refuse(
                    SessionFieldNames.Coach,
                    SessionRules.CoachBusyMessage(
                        $"{busyCoach.CoachFirstName} {busyCoach.CoachLastName}".Trim(),
                        when));
            }
        }
    }

    /// <summary>The occurrences a recurrence writes, the first one included.</summary>
    public static List<(DateTime StartsAt, DateTime EndsAt)> Occurrences(
        DateTime startsAt,
        DateTime endsAt,
        int recurrenceWeeks)
    {
        return Enumerable.Range(0, Math.Max(1, recurrenceWeeks))
            .Select(week => (startsAt.AddDays(week * 7), endsAt.AddDays(week * 7)))
            .ToList();
    }

    private static string WhenLabel(DateTime startsAt) =>
        $"le {startsAt:dddd d MMMM} à {startsAt:HH\\hmm}";
}
