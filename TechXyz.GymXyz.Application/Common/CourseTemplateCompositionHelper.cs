using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The one piece a course template is made of beyond its own columns: the
/// coaches allowed to run it. Create and update write it the same way, so the
/// rules live here once.
/// </summary>
public static class CourseTemplateCompositionHelper
{
    /// <summary>
    /// The requested coaches, in the order asked for. An id that resolves to
    /// nothing active is a broken request, not a silent omission.
    /// </summary>
    public static async Task<List<Coach>> LoadOrderedCoachesAsync(
        IGymDbContext dbContext,
        IReadOnlyList<int> coachIds,
        CancellationToken cancellationToken)
    {
        var requested = coachIds.Distinct().ToList();

        var known = await dbContext.Coaches
            .Where(coach => coach.IsActive && requested.Contains(coach.Id))
            .ToListAsync(cancellationToken);

        if (known.Count != requested.Count)
        {
            throw new ValidationException("Coach introuvable.");
        }

        return requested
            .Select(id => known.First(coach => coach.Id == id))
            .ToList();
    }

    /// <summary>
    /// Brings the template's coaches in line with <paramref name="coachIds"/>,
    /// in the order given — that is the order their avatars stack in. Existing
    /// links are kept and re-ranked rather than dropped and recreated, so the
    /// unique index never sees a duplicate mid-save.
    /// </summary>
    public static async Task SyncCoachesAsync(
        IGymDbContext dbContext,
        CourseTemplate template,
        IReadOnlyList<int> coachIds,
        CancellationToken cancellationToken)
    {
        var ordered = await LoadOrderedCoachesAsync(dbContext, coachIds, cancellationToken);
        var requested = ordered.Select(coach => coach.Id).ToList();

        var existing = await dbContext.CourseTemplateCoaches
            .Where(link => link.CourseTemplateId == template.Id)
            .ToListAsync(cancellationToken);

        var dropped = existing.Where(link => !requested.Contains(link.CoachId)).ToList();
        if (dropped.Count > 0)
        {
            // Detail rows of an edit, not a record the user deletes: they go
            // for good, and the unique (template, coach) index requires it.
            dbContext.CourseTemplateCoaches.RemoveRange(dropped);
        }

        for (var rank = 0; rank < requested.Count; rank++)
        {
            var coachId = requested[rank];
            var link = existing.FirstOrDefault(candidate => candidate.CoachId == coachId);

            if (link is null)
            {
                dbContext.CourseTemplateCoaches.Add(new CourseTemplateCoach
                {
                    CourseTemplateId = template.Id,
                    CoachId = coachId,
                    Rank = rank,
                    TenantId = template.TenantId
                });
            }
            else
            {
                link.Rank = rank;
                link.IsActive = true;
            }
        }
    }
}
