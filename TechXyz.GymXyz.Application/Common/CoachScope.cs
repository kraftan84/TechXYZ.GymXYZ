using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// How much of the gym's work the caller may see: all of it, or one coach's.
/// <para>
/// One place answers it so the Présences list, the Accueil counter and the
/// sidebar badge cannot disagree. A badge reading "2 séances à pointer" over a
/// list holding one is worse than either number alone — it reads as a lost
/// session rather than as a filter.
/// </para>
/// <para>
/// A value rather than a nullable id, because "no restriction" and "restricted
/// to nobody" are different answers that a plain <c>int?</c> spells the same
/// way. The second one is real: a coach account created from Réglages before
/// anybody linked it to a roster entry has no <c>Coach</c> row behind it, and
/// resolving that to "sees everything" is the exact bug this type exists to
/// make unwritable.
/// </para>
/// </summary>
public readonly record struct CoachScope(bool IsRestricted, int? CoachId)
{
    /// <summary>A manager, or a platform admin inside a customer: the whole gym.</summary>
    public static readonly CoachScope Unrestricted = new(IsRestricted: false, CoachId: null);

    /// <summary>
    /// Refused when a coach reaches for a session that is not theirs to run.
    /// Worded as a perimeter rather than as a mistake: they did nothing wrong,
    /// the class simply belongs to somebody else.
    /// </summary>
    public const string NotYourSession =
        "Cette séance est animée par un autre coach : elle ne vous appartient pas.";

    /// <summary>
    /// What this caller may see. Anybody who is not a manager is confined to
    /// their own coach row — to nothing at all when they have none.
    /// </summary>
    public static CoachScope For(ICurrentUserService currentUser) =>
        ManagerOnly.Holds(currentUser)
            ? Unrestricted
            : new CoachScope(IsRestricted: true, currentUser.CoachId);

    /// <summary>
    /// Whether this session is the caller's to read or to write. An unrestricted
    /// caller owns every session; an unlinked coach owns none, because
    /// <see cref="CoachId"/> is null and a session's coach never is once set.
    /// </summary>
    public bool Covers(Session session) => CoversCoach(session.CoachId);

    /// <summary>
    /// Whether work assigned to this coach is the caller's. Null — a slot with
    /// nobody running it, such as an open-gym hour — belongs to the gym, so a
    /// restricted caller does not own it.
    /// </summary>
    public bool CoversCoach(int? coachId) =>
        !IsRestricted || (coachId is { } coach && coach == CoachId);

    /// <summary>
    /// Narrows a session query to what this caller may see. Written as a single
    /// expression so it translates to SQL rather than pulling the gym's whole
    /// week into memory to filter it.
    /// </summary>
    public IQueryable<Session> Apply(IQueryable<Session> sessions)
    {
        if (!IsRestricted)
        {
            return sessions;
        }

        // Copied out of the struct: a lambda in a struct cannot capture `this`.
        var coachId = CoachId;

        return sessions.Where(session => session.CoachId != null && session.CoachId == coachId);
    }

    /// <summary>
    /// Narrows a member query to the people this caller has actually had in
    /// front of them — anybody booked onto one of their sessions, past or
    /// coming.
    /// <para>
    /// Applied to the details query as well as the list, or the
    /// <c>/members/{id}</c> URL walks straight around it. That is the whole
    /// reason this lives here instead of in the list handler.
    /// </para>
    /// </summary>
    public IQueryable<Member> ApplyToMembers(IQueryable<Member> members)
    {
        if (!IsRestricted)
        {
            return members;
        }

        var coachId = CoachId;

        return members.Where(member => member.Registrations!.Any(seat =>
            seat.IsActive
            && seat.Session!.IsActive
            && seat.Session.CoachId != null
            && seat.Session.CoachId == coachId));
    }
}
