using System.Linq.Expressions;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The member standing rule.
/// <para>
/// Everything hangs on one thing — the latest end date among the subscriptions
/// covering today. <see cref="Resolve"/> turns that value into the label the
/// user reads; <see cref="Matches"/> says the same three things over the entity
/// so the database can filter and count without loading rows.
/// </para>
/// <para>
/// The two are written separately because a predicate over a projected record
/// does not translate to SQL. <c>MemberStatusRulesTests</c> pins them to each
/// other through the real query, so they cannot drift.
/// </para>
/// <para>
/// Business decision of lot 1: the standing is derived from the subscription
/// alone. The "no visit for a while" leg joins at lot 6 (attendance) and the
/// whole thing moves onto Plan/Subscription/Payment at lot 7.
/// </para>
/// </summary>
public static class MemberStatusRules
{
    /// <summary>A cover ending within this many days reads "Expire bientôt".</summary>
    public const int ExpiringSoonWithinDays = 7;

    public static DateOnly HorizonFrom(DateOnly today) => today.AddDays(ExpiringSoonWithinDays);

    public static MemberStatus Resolve(DateOnly? currentSubscriptionEndsOn, DateOnly horizon)
    {
        if (currentSubscriptionEndsOn is null)
            return MemberStatus.Inactive;

        return currentSubscriptionEndsOn <= horizon
            ? MemberStatus.ExpiringSoon
            : MemberStatus.Active;
    }

    /// <summary>
    /// The same three conditions, over the entity. "Active" is a cover reaching
    /// past the horizon; "expiring soon" is a cover today but none past it;
    /// "inactive" is no cover at all.
    /// </summary>
    public static Expression<Func<Member, bool>> Matches(MemberStatus status, DateOnly today, DateOnly horizon)
    {
        return status switch
        {
            MemberStatus.Inactive => member => !member.Subscriptions!.Any(subscription =>
                subscription.IsActive &&
                subscription.StartDate <= today &&
                subscription.EndDate >= today),

            MemberStatus.ExpiringSoon => member =>
                member.Subscriptions!.Any(subscription =>
                    subscription.IsActive &&
                    subscription.StartDate <= today &&
                    subscription.EndDate >= today) &&
                !member.Subscriptions!.Any(subscription =>
                    subscription.IsActive &&
                    subscription.StartDate <= today &&
                    subscription.EndDate > horizon),

            // A cover ending past the horizon necessarily covers today.
            _ => member => member.Subscriptions!.Any(subscription =>
                subscription.IsActive &&
                subscription.StartDate <= today &&
                subscription.EndDate > horizon)
        };
    }
}
