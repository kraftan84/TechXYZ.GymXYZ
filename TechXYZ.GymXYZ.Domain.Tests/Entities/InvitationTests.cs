using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class InvitationTests
{
    [Fact]
    public void IsPending_ShouldBeTrue_WhileNobodyHasAnswered()
    {
        var invitation = new Invitation
        {
            Email = "theo.garnier@gymxyz.fr",
            RoleName = "Coach",
            SentOn = DateTime.UtcNow.AddDays(-2)
        };

        invitation.IsPending.ShouldBeTrue();
    }

    [Fact]
    public void IsPending_ShouldBeFalse_OnceTheInvitationIsTakenUp()
    {
        var invitation = new Invitation
        {
            Email = "camille.durand@gymxyz.fr",
            RoleName = "Member",
            SentOn = DateTime.UtcNow.AddDays(-5),
            AcceptedOn = DateTime.UtcNow
        };

        invitation.IsPending.ShouldBeFalse();
    }

    [Fact]
    public void IsPending_ShouldBeFalse_OnceTheInvitationIsWithdrawn()
    {
        // Withdrawing is the soft delete every entity uses, so an unanswered
        // invitation must stop counting as pending the moment it is retired.
        var invitation = new Invitation
        {
            Email = "theo.garnier@gymxyz.fr",
            RoleName = "Coach",
            SentOn = DateTime.UtcNow.AddDays(-2),
            IsActive = false
        };

        invitation.IsPending.ShouldBeFalse();
    }

    [Fact]
    public void MemberId_ShouldTellAMemberInvitationFromATeamOne()
    {
        var team = new Invitation { Email = "theo.garnier@gymxyz.fr", RoleName = "Coach" };
        var member = new Invitation { Email = "camille.durand@gymxyz.fr", RoleName = "Member", MemberId = 4 };

        team.MemberId.ShouldBeNull();
        member.MemberId.ShouldBe(4);
    }
}
