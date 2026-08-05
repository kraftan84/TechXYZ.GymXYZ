using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Components.Features.Planning;

/// <summary>
/// What the planning drawer binds to. A plain mutable holder, the way the other
/// drawers of the build work — the command is built from it on save.
/// </summary>
public sealed class SessionEditModel
{
    public int? Id { get; set; }

    public int CourseTemplateId { get; set; }
    public int LocationId { get; set; }
    public int? CoachId { get; set; }

    public DateTime Day { get; set; } = DateTime.Today;

    /// <summary>Kept apart from the day because the two are two controls.</summary>
    public TimeOnly Time { get; set; } = new(9, 0);

    /// <summary>Null follows the course's own capacity.</summary>
    public int? Capacity { get; set; }

    /// <summary>Weeks the course repeats over, itself included.</summary>
    public int RecurrenceWeeks { get; set; } = 1;

    /// <summary>How far an edit or a cancellation reaches inside a series.</summary>
    public SessionEditScope Scope { get; set; } = SessionEditScope.ThisOne;

    public string? CancellationReason { get; set; }

    /// <summary>True when the session belongs to a series, which is what unlocks the scope choice.</summary>
    public bool IsRecurring { get; set; }

    public DateTime StartsAt => Day.Date.Add(Time.ToTimeSpan());

    public static SessionEditModel ForCreate(DateOnly day, int? courseTemplateId, int? locationId) => new()
    {
        Day = day.ToDateTime(TimeOnly.MinValue),
        Time = new TimeOnly(9, 0),
        CourseTemplateId = courseTemplateId ?? 0,
        LocationId = locationId ?? 0
    };

    public static SessionEditModel From(PlanningSessionDto session) => new()
    {
        Id = session.Id,
        CourseTemplateId = session.CourseTemplateId,
        LocationId = session.LocationId,
        CoachId = session.CoachId,
        Day = session.StartsAt.Date,
        Time = TimeOnly.FromDateTime(session.StartsAt),
        Capacity = session.Capacity,
        IsRecurring = session.IsRecurring
    };
}
