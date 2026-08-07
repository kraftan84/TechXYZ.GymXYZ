using Shouldly;
using TechXyz.GymXyz.Application.Common;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class TeamAccessScopesTests
{
    [Theory]
    [InlineData(GymRoleNames.GymManager, TeamAccessScopes.Manager)]
    [InlineData(GymRoleNames.Coach, TeamAccessScopes.Coach)]
    [InlineData(GymRoleNames.Member, TeamAccessScopes.Member)]
    [InlineData(GymRoleNames.PlatformAdmin, TeamAccessScopes.PlatformAdmin)]
    public void Label_ShouldNameWhatTheRoleOpens(string role, string expected)
    {
        TeamAccessScopes.Label(role).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Gérante")]
    public void Label_ShouldFallBackToTheNarrowestScope_ForAnythingItDoesNotKnow(string? role)
    {
        // A free-text RoleLabel handed in by mistake must not read as
        // administration complète.
        TeamAccessScopes.Label(role).ShouldBe(TeamAccessScopes.Member);
    }

    [Fact]
    public void Assignable_ShouldNotLetACustomerGrantThePlatformRole()
    {
        TeamAccessScopes.Assignable.ShouldNotContain(GymRoleNames.PlatformAdmin);
        TeamAccessScopes.IsAssignable(GymRoleNames.PlatformAdmin).ShouldBeFalse();
    }

    [Fact]
    public void IsAssignable_ShouldAcceptTheTwoRolesTheSettingsScreenOffers()
    {
        TeamAccessScopes.IsAssignable(GymRoleNames.GymManager).ShouldBeTrue();
        TeamAccessScopes.IsAssignable(GymRoleNames.Coach).ShouldBeTrue();
        TeamAccessScopes.IsAssignable("Accueil").ShouldBeFalse();
        TeamAccessScopes.IsAssignable(null).ShouldBeFalse();
    }

    [Fact]
    public void Assignable_ShouldNotLetAGymGrantTheMemberRole()
    {
        // Members are reached by e-mail and never sign in, so an account
        // carrying this role has no screen to open. The name survives for the
        // invitations that already carry it — being grantable does not.
        TeamAccessScopes.Assignable.ShouldNotContain(GymRoleNames.Member);
        TeamAccessScopes.IsAssignable(GymRoleNames.Member).ShouldBeFalse();
    }

    [Fact]
    public void Label_ShouldStillAnswerForTheMemberRole()
    {
        // Withdrawn from the pickers, not from the vocabulary: a row seeded or
        // invited before the withdrawal still has to render a scope.
        TeamAccessScopes.Label(GymRoleNames.Member).ShouldBe(TeamAccessScopes.Member);
    }
}
