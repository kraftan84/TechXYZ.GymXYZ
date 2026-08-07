using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The half of the manager perimeter a caller cannot walk around. The page
/// attribute closes the screen; this closes the command, which is what a
/// hand-made request reaches when there is no screen in the way.
/// <para>
/// Both halves are needed and neither replaces the other: a screen with no
/// attribute leaks by URL, and a command with no guard leaks to anything that
/// can post. Until this lot only <c>ReopenAttendanceSheetCommand</c> had the
/// second half, so a coach who opened Réglages could promote themselves.
/// </para>
/// </summary>
public static class ManagerOnly
{
    /// <summary>
    /// Named as the refusal's field so the toast is built from <c>Errors</c>
    /// rather than arriving as "Validation invalide" — see
    /// <see cref="ValidationFailures.Refuse"/>.
    /// </summary>
    public const string Field = "L'accès";

    public const string Reserved =
        "Cette action est réservée au responsable de la salle.";

    /// <summary>
    /// A platform admin passes too, matching <c>GymPolicies.GymManager</c>,
    /// which admits both. Without it an admin inside a customer could open every
    /// screen and write on none of them — and the impersonation trail exists
    /// precisely so that visit can act.
    /// </summary>
    public static bool Holds(ICurrentUserService currentUser) =>
        currentUser.IsInRole(GymRoleNames.GymManager)
        || currentUser.IsInRole(GymRoleNames.PlatformAdmin);

    /// <summary>
    /// Asked before anything is loaded: whether the row exists is none of a
    /// coach's business either, and a refusal that varies with existence tells
    /// them what is there.
    /// </summary>
    public static void Require(ICurrentUserService currentUser)
    {
        if (Holds(currentUser))
            return;

        throw ValidationFailures.Refuse(Field, Reserved);
    }
}
