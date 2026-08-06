namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Standing of one subscription, as the suivi table's chip reads it. Always
/// derived, never stored: see <c>SubscriptionStatusRules</c> for the single
/// definition.
/// <para>
/// <see cref="MemberStatus"/> is a projection of this — the same four states in
/// the three words the members table uses. Deriving both from one rule is what
/// stops the two screens saying different things about the same person.
/// </para>
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Covered, past the warning window, and with credits left if it counts them.</summary>
    Active,

    /// <summary>Covered still, but the cover ends within the window — or the pack is nearly spent.</summary>
    ExpiringSoon,

    /// <summary>Over, and money is outstanding: a payment rejected or still pending.</summary>
    Late,

    /// <summary>Over, and nothing owed.</summary>
    Ended
}
