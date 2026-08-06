using System.Linq.Expressions;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The member standing rule — now a projection of <see cref="SubscriptionStatusRules"/>
/// rather than a second opinion on the same dates.
/// <para>
/// Lot 1 derived the standing from the cover's end date alone, which was all
/// there was. Lot 7 gives a subscription four states where the member has three,
/// and they overlap on every one of them: left as two rules reading the same
/// dates, the members table and the suivi table would disagree the first time
/// either was touched. So there is one rule, and this is the vocabulary the
/// members table speaks it in.
/// </para>
/// <para>
/// <c>Late</c> and <c>Ended</c> both land on "Inactif", which is what the
/// prototype draws: Théo Garnier reads "En retard" on the abonnements screen and
/// "Inactif" on the members table, same person, same day. The members table has
/// no fourth chip and lot 7 does not add one.
/// </para>
/// <para>
/// Business decision of lot 1, unchanged: the standing is derived from the
/// subscription alone. Attendance arrived at lot 6 without adding an inactivity
/// leg here, and poor attendance still surfaces where the prototype puts it —
/// the assiduité column and the "absents à relancer" card — rather than as a
/// standing nothing in the design asks for.
/// </para>
/// </summary>
public static class MemberStatusRules
{
    /// <inheritdoc cref="SubscriptionStatusRules.ExpiringSoonWithinDays"/>
    public const int ExpiringSoonWithinDays = SubscriptionStatusRules.ExpiringSoonWithinDays;

    public static DateOnly HorizonFrom(DateOnly today) => SubscriptionStatusRules.HorizonFrom(today);

    /// <summary>
    /// The three words the members table uses, for the cover that governs the
    /// member. No cover at all is "Inactif", the same as a finished one.
    /// </summary>
    public static MemberStatus Resolve(
        IEnumerable<SubscriptionCoverDto> covers,
        DateOnly today,
        DateOnly horizon)
    {
        var governing = SubscriptionStatusRules.Governing(covers, today, horizon);

        return governing is null
            ? MemberStatus.Inactive
            : Project(SubscriptionStatusRules.Resolve(governing, today, horizon));
    }

    /// <summary>The projection itself, in one place so it can be read in one line.</summary>
    public static MemberStatus Project(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Active => MemberStatus.Active,
        SubscriptionStatus.ExpiringSoon => MemberStatus.ExpiringSoon,
        _ => MemberStatus.Inactive
    };

    /// <summary>
    /// The same three conditions, over the entity. A member is "Actif" if any of
    /// their subscriptions is; "Expire bientôt" if one is and none is better;
    /// "Inactif" if none is either — which is the projection said backwards, and
    /// <c>MemberStatusRulesTests</c> pins it to <see cref="Resolve"/> through the
    /// real query.
    /// </summary>
    public static Expression<Func<Member, bool>> Matches(MemberStatus status, DateOnly today, DateOnly horizon)
    {
        var isActive = SubscriptionStatusRules.Matches(SubscriptionStatus.Active, today, horizon);
        var isExpiringSoon = SubscriptionStatusRules.Matches(SubscriptionStatus.ExpiringSoon, today, horizon);

        return status switch
        {
            MemberStatus.Inactive => member =>
                !member.Subscriptions!.AsQueryable().Any(isActive) &&
                !member.Subscriptions!.AsQueryable().Any(isExpiringSoon),

            MemberStatus.ExpiringSoon => member =>
                member.Subscriptions!.AsQueryable().Any(isExpiringSoon) &&
                !member.Subscriptions!.AsQueryable().Any(isActive),

            _ => member => member.Subscriptions!.AsQueryable().Any(isActive)
        };
    }
}
