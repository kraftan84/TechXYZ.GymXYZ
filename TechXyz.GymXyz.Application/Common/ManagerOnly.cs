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
    /// The gym's manager, and nobody else.
    /// <para>
    /// A platform admin used to pass here too, matching
    /// <c>GymPolicies.GymManager</c>, because an admin inside a customer had to
    /// be able to act as well as look. That visit was removed, and with it the
    /// only case the exception served: an admin now inhabits no gym, so no gym's
    /// commands are theirs to run. The platform's own commands are not guarded by
    /// this — they carry <c>IPlatformScoped</c> and sit behind the
    /// <c>PlatformAdmin</c> policy instead.
    /// </para>
    /// </summary>
    public static bool Holds(ICurrentUserService currentUser) =>
        currentUser.IsInRole(GymRoleNames.GymManager);

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
