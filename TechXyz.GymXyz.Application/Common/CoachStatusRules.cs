using System.Linq.Expressions;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The coach standing rule. Everything hangs on one value —
/// <c>Coach.AwayUntil</c>. <see cref="Resolve"/> turns it into the label the
/// user reads; <see cref="Matches"/> says the same thing over the entity so the
/// database can filter and count without loading rows.
/// <para>
/// The two are written separately because a predicate over a projected record
/// does not translate to SQL — the same reason as
/// <see cref="MemberStatusRules"/>, and <c>CoachStatusRulesTests</c> pins them
/// to each other through the real query.
/// </para>
/// </summary>
public static class CoachStatusRules
{
    /// <summary>
    /// A leave covers its last day: a coach away "jusqu'au 15 juin" is back on
    /// the 16th, and a date already past means nothing at all.
    /// </summary>
    public static CoachStatus Resolve(DateOnly? awayUntil, DateOnly today)
        => awayUntil is { } until && until >= today
            ? CoachStatus.Away
            : CoachStatus.Available;

    /// <summary>The same condition, over the entity.</summary>
    public static Expression<Func<Coach, bool>> Matches(CoachStatus status, DateOnly today)
        => status == CoachStatus.Away
            ? coach => coach.AwayUntil != null && coach.AwayUntil >= today
            : coach => coach.AwayUntil == null || coach.AwayUntil < today;
}
