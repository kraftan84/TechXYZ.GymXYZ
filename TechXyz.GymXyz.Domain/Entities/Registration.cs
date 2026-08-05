using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// A member's seat on a session. Counting these rows is what every occupancy
/// figure in the application means — the "14/20" on a planning block, the
/// average fill of a venue, a coach's week.
/// </summary>
/// <remarks>
/// The attendance side of the row — was the member there, late, absent, and when
/// they checked in — belongs to the same table by design, but arrives with the
/// attendance sheet at lot 6. Nothing here could write it today.
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
}
