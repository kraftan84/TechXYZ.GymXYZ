using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A member's seat on a session. Counting these rows is what every occupancy
/// figure in the application means — the "14/20" on a planning block, the
/// average fill of a venue, a coach's week.
/// </summary>
/// <remarks>
/// The row carries the seat and the attendance both, by design: the sheet is
/// the list of registrations and the status is one of its columns.
/// </remarks>
public class Registration : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public int SessionId { get; set; }
    public Session? Session { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// The session was full when the member signed up. A waiting seat is not an
    /// occupied one, so it never counts towards occupancy.
    /// </summary>
    public bool IsWaitlisted { get; set; }

    /// <summary>
    /// What the sheet recorded. Every seat starts <see cref="AttendanceStatus.Pending"/>,
    /// which is why an attendance rate counts only the seats somebody actually
    /// pointed.
    /// </summary>
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Pending;

    /// <summary>
    /// When the member was marked in. Set for <see cref="AttendanceStatus.Present"/>
    /// and <see cref="AttendanceStatus.Late"/>, cleared otherwise — an absence has
    /// no arrival time, and leaving a stale one behind would make "dernière venue"
    /// lie.
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// The credit pack this seat took an entry from, or null if it took none.
    /// <para>
    /// This stamp is the whole of the idempotence the model asks for: pointing a
    /// pack debits once, and re-pointing the same seat finds the stamp already
    /// set and does nothing. The natural key is the seat, not the tap — both
    /// <c>MarkAttendanceCommand</c> and <c>MarkWholeSheetCommand</c> can run
    /// twice over the same registration, and a counter that only happened to be
    /// right would not survive either.
    /// </para>
    /// <para>
    /// Clearing it gives the entry back: a seat corrected from present to absent
    /// did not consume a session.
    /// </para>
    /// </summary>
    public int? CreditDebitedFromSubscriptionId { get; set; }
}
