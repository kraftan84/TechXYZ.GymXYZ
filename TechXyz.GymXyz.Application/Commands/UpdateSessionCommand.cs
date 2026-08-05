using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Moves or re-staffs a session. Returns false when the id resolves to nothing.
/// <para>
/// With <see cref="SessionEditScope.ThisAndFollowing"/> the same change is
/// applied to every later occurrence of the series, keeping each one's own
/// offset from this one — moving a Tuesday class an hour later moves every
/// Tuesday after it, not the whole series onto one date.
/// </para>
/// </summary>
public sealed class UpdateSessionCommand : IRequest<bool>
{
    public UpdateSessionCommand(
        int id,
        int locationId,
        DateTime startsAt,
        int? coachId = null,
        int? capacity = null,
        SessionEditScope scope = SessionEditScope.ThisOne)
    {
        Id = id;
        LocationId = locationId;
        StartsAt = startsAt;
        CoachId = coachId;
        Capacity = capacity;
        Scope = scope;
    }

    public int Id { get; }
    public int LocationId { get; }
    public DateTime StartsAt { get; }
    public int? CoachId { get; }

    /// <summary>Null keeps the capacity the session already carries.</summary>
    public int? Capacity { get; }

    public SessionEditScope Scope { get; }
}
