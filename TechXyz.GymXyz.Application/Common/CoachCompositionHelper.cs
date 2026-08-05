using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The three pieces a coach is made of beyond its own columns: the weekly
/// availability, the disciplines and the certifications. Create and update
/// write them the same way, so the rules live here once.
/// </summary>
public static class CoachCompositionHelper
{
    public const int AvailabilityDayCount = 7;

    /// <summary>
    /// Seven flags, Monday to Sunday. A shorter list fills the missing days
    /// with "unavailable"; null leaves a brand-new coach available every day,
    /// which is the only assumption that does not lock someone out of the
    /// planning by default.
    /// </summary>
    public static void ApplyAvailability(Coach coach, IReadOnlyList<bool>? availability)
    {
        var days = availability is null
            ? Enumerable.Repeat(true, AvailabilityDayCount).ToArray()
            : Enumerable.Range(0, AvailabilityDayCount)
                .Select(index => index < availability.Count && availability[index])
                .ToArray();

        coach.AvailableOnMonday = days[0];
        coach.AvailableOnTuesday = days[1];
        coach.AvailableOnWednesday = days[2];
        coach.AvailableOnThursday = days[3];
        coach.AvailableOnFriday = days[4];
        coach.AvailableOnSaturday = days[5];
        coach.AvailableOnSunday = days[6];
    }

    /// <summary>
    /// The requested disciplines, in the order asked for. An id that resolves to
    /// nothing active is a broken request, not a silent omission.
    /// </summary>
    public static async Task<List<Discipline>> LoadOrderedDisciplinesAsync(
        IGymDbContext dbContext,
        IReadOnlyList<int> disciplineIds,
        CancellationToken cancellationToken)
    {
        var requested = disciplineIds.Distinct().ToList();

        var known = await dbContext.Disciplines
            .Where(discipline => discipline.IsActive && requested.Contains(discipline.Id))
            .ToListAsync(cancellationToken);

        if (known.Count != requested.Count)
        {
            throw new ValidationException("Discipline introuvable.");
        }

        return requested
            .Select(id => known.First(discipline => discipline.Id == id))
            .ToList();
    }

    /// <summary>
    /// Brings the coach's disciplines in line with <paramref name="disciplineIds"/>,
    /// in the order given — the first one carries the brand pill. Existing links
    /// are kept and re-ranked rather than dropped and recreated, so the unique
    /// index never sees a duplicate mid-save.
    /// </summary>
    public static async Task SyncDisciplinesAsync(
        IGymDbContext dbContext,
        Coach coach,
        IReadOnlyList<int> disciplineIds,
        CancellationToken cancellationToken)
    {
        var ordered = await LoadOrderedDisciplinesAsync(dbContext, disciplineIds, cancellationToken);
        var requested = ordered.Select(discipline => discipline.Id).ToList();

        var existing = await dbContext.CoachDisciplines
            .Where(link => link.CoachId == coach.Id)
            .ToListAsync(cancellationToken);

        var dropped = existing.Where(link => !requested.Contains(link.DisciplineId)).ToList();
        if (dropped.Count > 0)
        {
            // Detail rows of an edit, not a record the user deletes: they go
            // for good, and the unique (coach, discipline) index requires it.
            dbContext.CoachDisciplines.RemoveRange(dropped);
        }

        for (var rank = 0; rank < requested.Count; rank++)
        {
            var disciplineId = requested[rank];
            var link = existing.FirstOrDefault(candidate => candidate.DisciplineId == disciplineId);

            if (link is null)
            {
                dbContext.CoachDisciplines.Add(new CoachDiscipline
                {
                    CoachId = coach.Id,
                    DisciplineId = disciplineId,
                    Rank = rank,
                    TenantId = coach.TenantId
                });
            }
            else
            {
                link.Rank = rank;
                link.IsActive = true;
            }
        }
    }

    /// <summary>
    /// Replaces the certification list, in the order given. Blank lines are
    /// dropped rather than stored as empty rows.
    /// </summary>
    public static async Task SyncCertificationsAsync(
        IGymDbContext dbContext,
        Coach coach,
        IReadOnlyList<string> certifications,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.CoachCertifications
            .Where(certification => certification.CoachId == coach.Id)
            .ToListAsync(cancellationToken);

        OrderedLabelHelper.Replace(
            dbContext.CoachCertifications,
            existing,
            OrderedLabelHelper.Normalize(certifications),
            (label, rank) => new CoachCertification(label)
            {
                CoachId = coach.Id,
                Rank = rank,
                TenantId = coach.TenantId
            });
    }
}
