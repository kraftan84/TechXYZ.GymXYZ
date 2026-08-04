using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The standing is written twice: <c>Resolve</c> produces the label, <c>Matches</c>
/// filters in SQL. These tests pin them to each other — on the relational
/// provider, so a rule that cannot be translated fails here rather than in
/// production.
/// </summary>
public class MemberStatusRulesTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Horizon = MemberStatusRules.HorizonFrom(Today);

    [Fact]
    public void HorizonFrom_ShouldSitSevenDaysAhead()
    {
        MemberStatusRules.ExpiringSoonWithinDays.ShouldBe(7);
        Horizon.ShouldBe(Today.AddDays(7));
    }

    [Theory]
    [InlineData(null, MemberStatus.Inactive)]
    [InlineData(0, MemberStatus.ExpiringSoon)]
    [InlineData(1, MemberStatus.ExpiringSoon)]
    [InlineData(7, MemberStatus.ExpiringSoon)]
    [InlineData(8, MemberStatus.Active)]
    [InlineData(120, MemberStatus.Active)]
    public void Resolve_ShouldReadTheEndOfTheCurrentCover(int? endsInDays, MemberStatus expected)
    {
        var endsOn = endsInDays is { } days ? Today.AddDays(days) : (DateOnly?)null;

        MemberStatusRules.Resolve(endsOn, Horizon).ShouldBe(expected);
    }

    [Fact]
    public async Task Matches_ShouldAgreeWithResolve_OnEveryBoundary_OnSqlite()
    {
        // One member per boundary of the rule, including the two edges of the
        // warning window and both sides of "no cover at all".
        int?[] boundaries = [null, -1, 0, 1, 7, 8, 120];

        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        foreach (var endsInDays in boundaries)
        {
            var member = new Member("Membre", NameFor(endsInDays));

            if (endsInDays is { } days)
            {
                member.Subscriptions =
                [
                    new Subscription { StartDate = Today.AddMonths(-2), EndDate = Today.AddDays(days) }
                ];
            }

            dbContext.Members.Add(member);
        }

        await dbContext.SaveChangesAsync();

        var handler = new GetMembersQueryHandler(dbContext);
        var everyone = await handler.Handle(new GetMembersQuery(), CancellationToken.None);

        everyone.Items.Count.ShouldBe(boundaries.Length);

        // 1. The label each row carries is Resolve applied to its own cover.
        foreach (var item in everyone.Items)
        {
            item.Status.ShouldBe(MemberStatusRules.Resolve(item.CurrentSubscriptionEndsOn, Horizon));
        }

        // 2. Filtering in SQL selects exactly the rows carrying that label.
        foreach (var status in Enum.GetValues<MemberStatus>())
        {
            var filtered = await handler.Handle(new GetMembersQuery { Status = status }, CancellationToken.None);

            filtered.Items.Select(item => item.LastName).OrderBy(name => name)
                .ShouldBe(
                    everyone.Items.Where(item => item.Status == status)
                        .Select(item => item.LastName).OrderBy(name => name),
                    $"the SQL filter for {status} must return exactly the members carrying that label.");
        }

        // 3. The three counts partition the list, no row counted twice or lost.
        everyone.ActiveCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.Active));
        everyone.ExpiringSoonCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.ExpiringSoon));
        everyone.InactiveCount.ShouldBe(everyone.Items.Count(item => item.Status == MemberStatus.Inactive));
        (everyone.ActiveCount + everyone.ExpiringSoonCount + everyone.InactiveCount)
            .ShouldBe(everyone.TotalCount);
    }

    private static string NameFor(int? endsInDays)
        => endsInDays is { } days ? $"J{days + 100:D3}" : "Aucun";
}
