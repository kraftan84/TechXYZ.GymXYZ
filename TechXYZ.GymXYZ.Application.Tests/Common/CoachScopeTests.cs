using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXYZ.GymXYZ.Application.Tests.Members;

namespace TechXYZ.GymXYZ.Application.Tests.Common;

/// <summary>
/// The one place that answers "whose work is this". Every filter and every
/// session guard in the lot reads it, so the two answers it must never confuse
/// — "no restriction" and "restricted to nobody" — are pinned here rather than
/// once per handler.
/// </summary>
public class CoachScopeTests
{
    private const int Nora = 7;
    private const int Samir = 9;

    [Fact]
    public void For_ShouldNotRestrictAManager()
    {
        var scope = CoachScope.For(TestCurrentUserService.Manager());

        scope.IsRestricted.ShouldBeFalse();
        scope.Covers(SessionOf(Nora)).ShouldBeTrue();
        scope.Covers(SessionOf(null)).ShouldBeTrue();
    }

    [Fact]
    public void For_ShouldNotRestrictAPlatformAdmin()
    {
        // Inside a customer an admin stands in for its manager, the same way
        // GymPolicies.GymManager admits them.
        var scope = CoachScope.For(new TestCurrentUserService(GymRoleNames.PlatformAdmin));

        scope.IsRestricted.ShouldBeFalse();
    }

    [Fact]
    public void For_ShouldRestrictACoachToTheirOwnSessions()
    {
        var scope = CoachScope.For(TestCurrentUserService.Coach(Nora));

        scope.Covers(SessionOf(Nora)).ShouldBeTrue();
        scope.Covers(SessionOf(Samir)).ShouldBeFalse();
    }

    [Fact]
    public void For_ShouldRestrictACoachToNothing_WhenNoCoachRowIsBehindTheAccount()
    {
        // The bug this type exists to make unwritable: an account created from
        // Réglages and never linked to a roster entry has no coach id, and
        // reading that as "no restriction" would hand them the whole gym.
        var scope = CoachScope.For(new TestCurrentUserService(GymRoleNames.Coach));

        scope.IsRestricted.ShouldBeTrue();
        scope.Covers(SessionOf(Nora)).ShouldBeFalse();
        scope.Covers(SessionOf(null)).ShouldBeFalse();
    }

    [Fact]
    public void Covers_ShouldRefuseASessionNobodyRuns_ForACoach()
    {
        // An open-gym hour has no coach. It belongs to the gym, so it is not a
        // coach's to point or to call off.
        CoachScope.For(TestCurrentUserService.Coach(Nora))
            .Covers(SessionOf(null))
            .ShouldBeFalse();
    }

    [Fact]
    public void CoversCoach_ShouldRefuseHandingWorkToSomebodyElse()
    {
        var scope = CoachScope.For(TestCurrentUserService.Coach(Nora));

        scope.CoversCoach(Nora).ShouldBeTrue();
        scope.CoversCoach(Samir).ShouldBeFalse();
        scope.CoversCoach(null).ShouldBeFalse();
    }

    [Fact]
    public void Apply_ShouldKeepOnlyTheCoachsSessions()
    {
        var sessions = new[] { SessionOf(Nora), SessionOf(Samir), SessionOf(null) }.AsQueryable();

        CoachScope.For(TestCurrentUserService.Coach(Nora))
            .Apply(sessions)
            .Select(session => session.CoachId)
            .ShouldBe([Nora]);
    }

    [Fact]
    public void Apply_ShouldChangeNothing_ForAManager()
    {
        var sessions = new[] { SessionOf(Nora), SessionOf(Samir), SessionOf(null) }.AsQueryable();

        CoachScope.For(TestCurrentUserService.Manager())
            .Apply(sessions)
            .Count()
            .ShouldBe(3);
    }

    private static Session SessionOf(int? coachId) => new() { CoachId = coachId };
}
