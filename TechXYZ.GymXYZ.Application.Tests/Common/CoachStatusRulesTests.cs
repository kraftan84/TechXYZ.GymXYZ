using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The coach standing is written twice: <c>Resolve</c> produces the label,
/// <c>Matches</c> filters in SQL. These tests pin them to each other — on the
/// relational provider, so a rule that cannot be translated fails here rather
/// than in production.
/// </summary>
public class CoachStatusRulesTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Theory]
    [InlineData(null, CoachStatus.Available)]
    [InlineData(-30, CoachStatus.Available)]
    [InlineData(-1, CoachStatus.Available)]
    [InlineData(0, CoachStatus.Away)]
    [InlineData(1, CoachStatus.Away)]
    [InlineData(45, CoachStatus.Away)]
    public void Resolve_ShouldReadTheEndOfTheLeave(int? awayInDays, CoachStatus expected)
    {
        var awayUntil = awayInDays is { } days ? Today.AddDays(days) : (DateOnly?)null;

        CoachStatusRules.Resolve(awayUntil, Today).ShouldBe(expected);
    }

    [Fact]
    public async Task Matches_ShouldAgreeWithResolve_OnEveryBoundary_OnSqlite()
    {
        // One coach per boundary of the rule: no leave at all, a leave that has
        // run out, one ending today, and two still running.
        int?[] boundaries = [null, -30, -1, 0, 1, 45];

        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        foreach (var awayInDays in boundaries)
        {
            dbContext.Coaches.Add(new Coach("Coach", NameFor(awayInDays))
            {
                AwayUntil = awayInDays is { } days ? Today.AddDays(days) : null
            });
        }

        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);
        var everyone = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        everyone.Items.Count.ShouldBe(boundaries.Length);

        // 1. The label each card carries is Resolve applied to its own leave.
        foreach (var item in everyone.Items)
        {
            item.Status.ShouldBe(CoachStatusRules.Resolve(item.AwayUntil, Today));
        }

        // 2. Filtering in SQL selects exactly the rows carrying that label.
        //
        // Only the two standings the database can answer. "Cours pleins" is
        // counted from sessions and is a refinement of being available, so it is
        // a label the card wears, never a chip the grid filters on — the case
        // below pins that.
        foreach (var status in new[] { CoachStatus.Available, CoachStatus.Away })
        {
            var filtered = await handler.Handle(new GetCoachesQuery { Status = status }, CancellationToken.None);

            filtered.Items.Select(item => item.LastName).OrderBy(name => name)
                .ShouldBe(
                    everyone.Items.Where(item => item.Status == status)
                        .Select(item => item.LastName).OrderBy(name => name),
                    $"the SQL filter for {status} must return exactly the coaches carrying that label.");
        }

        // 3. The two counts partition the grid, no row counted twice or lost.
        everyone.AvailableCount.ShouldBe(everyone.Items.Count(item => item.Status != CoachStatus.Away));
        everyone.AwayCount.ShouldBe(everyone.Items.Count(item => item.Status == CoachStatus.Away));
        (everyone.AvailableCount + everyone.AwayCount).ShouldBe(everyone.TotalCount);
    }

    /// <summary>
    /// A coach whose classes fill up is still a coach you can book: the chip
    /// says "Cours pleins", and the "Disponibles" filter keeps them. Filtering on
    /// the value itself is not a thing the grid offers, and asking for it
    /// behaves as asking for available rather than returning nothing.
    /// </summary>
    [Fact]
    public void Resolve_ShouldPreferTheLeaveOverAFullWeek()
    {
        CoachStatusRules.Resolve(Today.AddDays(3), Today, fillRate: 100).ShouldBe(CoachStatus.Away);
        CoachStatusRules.Resolve(null, Today, fillRate: 95).ShouldBe(CoachStatus.FullClasses);
        CoachStatusRules.Resolve(null, Today, fillRate: 89).ShouldBe(CoachStatus.Available);
        CoachStatusRules.Resolve(null, Today, fillRate: null).ShouldBe(CoachStatus.Available);
    }

    private static string NameFor(int? awayInDays)
        => awayInDays is { } days ? $"J{days + 100:D3}" : "Present";
}
