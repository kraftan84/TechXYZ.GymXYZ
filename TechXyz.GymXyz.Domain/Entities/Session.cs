using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A course as it actually happens: "Power Cycle, Friday the 12th at 18:00,
/// Studio C, Nora". The catalogue entry it comes from is <see cref="CourseTemplate"/>,
/// and the two are deliberately separate — one row per occurrence is what the
/// planning grid reads, what occupancy counts, and what attendance is marked
/// against.
/// </summary>
public class Session : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int CourseTemplateId { get; set; }
    public CourseTemplate? CourseTemplate { get; set; }

    /// <summary>
    /// Who runs it. Optional: an open-access slot such as "Open Gym" is on the
    /// planning without anybody animating it.
    /// </summary>
    public int? CoachId { get; set; }
    public Coach? Coach { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    /// <summary>
    /// Copied from the template when the session is created. Editing a course in
    /// the catalogue must not rewrite the history of the sessions already run.
    /// A capacity of one is what makes the session private, the same rule the
    /// catalogue uses.
    /// </summary>
    public int Capacity { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

    /// <summary>Why the session was called off, shown to the members registered.</summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Ties together the occurrences created in one go, so "this one and all the
    /// following" is a single query. Null when the session was created on its own.
    /// The rule itself is not stored: the rows are the truth.
    /// </summary>
    public Guid? SeriesId { get; set; }

    /// <summary>
    /// When the attendance sheet was validated. Null means it is still open, and
    /// that single field is what "pointée" reads: the state is derived from it
    /// rather than stored twice.
    /// <para>
    /// Once set, the sheet is read-only. Only a <c>GymManager</c> can clear it,
    /// through <c>ReopenAttendanceSheetCommand</c>.
    /// </para>
    /// </summary>
    public DateTime? AttendanceClosedAt { get; set; }

    /// <summary>
    /// Who reopened a validated sheet, and when. Reopening is the one way to
    /// rewrite a record that was already closed, so it leaves a trace rather
    /// than happening silently.
    /// </summary>
    public string? AttendanceReopenedBy { get; set; }

    /// <inheritdoc cref="AttendanceReopenedBy"/>
    public DateTime? AttendanceReopenedAt { get; set; }

    public ICollection<Registration>? Registrations { get; set; }
}
