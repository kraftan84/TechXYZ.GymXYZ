using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class GymSettingsQueryHandlerTests
{
    [Fact]
    public async Task GetGymSettings_ShouldReadTheIdentityOffTheTenant()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldReadTheIdentityOffTheTenant));

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Identity.Name.ShouldBe("GymXYZ");
        result.Identity.ZipCode.ShouldBe("69003");
        result.Identity.Capacity.ShouldBe(180);
        result.Identity.WorksFromAnArea.ShouldBeFalse();
    }

    [Fact]
    public async Task GetGymSettings_ShouldFlagACustomerWorkingFromAnArea()
    {
        await using var dbContext = await SeededAsync(
            nameof(GetGymSettings_ShouldFlagACustomerWorkingFromAnArea),
            tenant =>
            {
                tenant.AreaLabel = "Thonon et alentours";
                tenant.Street = null;
                tenant.ZipCode = null;
                tenant.City = null;
            });

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Identity.WorksFromAnArea.ShouldBeTrue();
        result.Identity.AreaLabel.ShouldBe("Thonon et alentours");
    }

    [Fact]
    public async Task GetGymSettings_ShouldReadTheDefaults_WhenTheCustomerHasNoSettingsRow()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldReadTheDefaults_WhenTheCustomerHasNoSettingsRow));

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Payments.Currency.ShouldBe(GymSettings.DefaultCurrency);
        result.OpeningHours.ShouldBeEmpty();

        // A missing row is not a decision: all six come back, at their defaults.
        result.Notifications.Count.ShouldBe(NotificationDefaults.All.Count);
        result.Notifications
            .Single(setting => setting.Key == NotificationKey.RenewalReminder)
            .IsEnabled.ShouldBeTrue();
        result.Notifications.ShouldAllBe(setting => setting.Id == 0);
    }

    [Fact]
    public async Task GetGymSettings_ShouldTopUpTheMissingNotificationsOnly()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldTopUpTheMissingNotificationsOnly));

        // The gym has decided about one message; the other five have no row.
        dbContext.NotificationSettings.Add(new NotificationSetting
        {
            Group = NotificationGroup.MembersAndSubscriptions,
            Key = NotificationKey.RenewalReminder,
            IsEnabled = false,
            Channels = NotificationChannels.Email
        });
        await dbContext.SaveChangesAsync();

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Notifications.Count.ShouldBe(NotificationDefaults.All.Count);

        var stored = result.Notifications.Single(setting => setting.Key == NotificationKey.RenewalReminder);
        stored.IsEnabled.ShouldBeFalse();
        stored.Id.ShouldNotBe(0);
    }

    [Fact]
    public async Task GetGymSettings_ShouldReturnTheNotificationsInPanelOrder()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldReturnTheNotificationsInPanelOrder));

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Notifications.Select(setting => setting.Key)
            .ShouldBe(NotificationDefaults.All.Select(entry => entry.Key).ToList());

        result.InGroup(NotificationGroup.CoursesAndAttendance).Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetGymSettings_ShouldOrderTheOpeningHoursByRank()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldOrderTheOpeningHoursByRank));

        var settings = new GymSettings();
        settings.AddOpeningHours(new OpeningHours
        {
            DayFrom = DayOfWeek.Monday, DayTo = DayOfWeek.Friday,
            OpensAt = new(6, 30), ClosesAt = new(22, 0)
        });
        settings.AddOpeningHours(new OpeningHours
        {
            DayFrom = DayOfWeek.Sunday, DayTo = DayOfWeek.Sunday,
            OpensAt = new(9, 0), ClosesAt = new(13, 0)
        });
        dbContext.GymSettings.Add(settings);
        await dbContext.SaveChangesAsync();

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.OpeningHours.Select(hours => hours.DayFrom)
            .ShouldBe([DayOfWeek.Monday, DayOfWeek.Sunday]);
    }

    [Fact]
    public async Task GetGymSettings_ShouldSplitTheTeamFromTheMemberAccounts()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldSplitTheTeamFromTheMemberAccounts));

        var directory = new TestUserDirectory()
            .Add("manager", "dwayne@gymxyz.fr", GymRoleNames.GymManager)
            .Add("coach", "nora@gymxyz.fr", GymRoleNames.Coach)
            .Add("membre", "laetitia@gymxyz.fr", GymRoleNames.Member);

        var result = await Handler(dbContext, directory)
            .Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Team.Team.Select(person => person.Email)
            .ShouldBe(["dwayne@gymxyz.fr", "nora@gymxyz.fr"]);

        // The manager sorts first whatever the alphabet says.
        result.Team.Team[0].Role.ShouldBe(GymRoleNames.GymManager);
        result.Team.Team[0].AccessScope.ShouldBe(TeamAccessScopes.Manager);
        result.Team.Team[1].AccessScope.ShouldBe(TeamAccessScopes.Coach);
    }

    [Fact]
    public async Task GetGymSettings_ShouldMarkTheSignedInPerson()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldMarkTheSignedInPerson));

        var directory = new TestUserDirectory()
            .Add("manager", "dwayne@gymxyz.fr", GymRoleNames.GymManager)
            .Add("coach", "nora@gymxyz.fr", GymRoleNames.Coach);

        var result = await Handler(dbContext, directory, currentUser: "dwayne@gymxyz.fr")
            .Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Team.Team.Single(person => person.IsCurrentUser).Email.ShouldBe("dwayne@gymxyz.fr");
    }

    [Fact]
    public async Task GetGymSettings_ShouldReadEachMemberAccessState()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldReadEachMemberAccessState));

        var active = new Member("Laetitia", "Moriceau") { Email = "laetitia@gymxyz.fr", UserId = "membre" };
        var invited = new Member("Camille", "Durand") { Email = "camille@gymxyz.fr" };
        var without = new Member("Théo", "Garnier") { Email = "theo@gymxyz.fr" };

        dbContext.Members.AddRange(active, invited, without);
        await dbContext.SaveChangesAsync();

        dbContext.Invitations.Add(new Invitation
        {
            Email = "camille@gymxyz.fr",
            RoleName = GymRoleNames.Member,
            MemberId = invited.Id,
            SentOn = DateTime.UtcNow.AddDays(-5)
        });
        await dbContext.SaveChangesAsync();

        var directory = new TestUserDirectory()
            .Add("manager", "dwayne@gymxyz.fr", GymRoleNames.GymManager)
            .Add("membre", "laetitia@gymxyz.fr", GymRoleNames.Member);

        var result = await Handler(dbContext, directory)
            .Handle(new GetGymSettingsQuery(), CancellationToken.None);

        var accounts = result.Team.MemberAccounts.ToDictionary(account => account.LastName);
        accounts["Moriceau"].State.ShouldBe(MemberAccessState.Active);
        accounts["Durand"].State.ShouldBe(MemberAccessState.Invited);
        accounts["Garnier"].State.ShouldBe(MemberAccessState.None);

        result.Team.MemberAccess.Total.ShouldBe(3);
        result.Team.MemberAccess.WithAccount.ShouldBe(1);
        result.Team.MemberAccess.Invited.ShouldBe(1);
        result.Team.MemberAccess.WithoutAccess.ShouldBe(1);
    }

    [Fact]
    public async Task GetGymSettings_ShouldKeepAMemberInvitationOutOfTheTeamList()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldKeepAMemberInvitationOutOfTheTeamList));

        var member = new Member("Camille", "Durand") { Email = "camille@gymxyz.fr" };
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        dbContext.Invitations.AddRange(
            new Invitation
            {
                Email = "camille@gymxyz.fr", RoleName = GymRoleNames.Member,
                MemberId = member.Id, SentOn = DateTime.UtcNow
            },
            new Invitation
            {
                Email = "theo@gymxyz.fr", RoleName = GymRoleNames.Coach, SentOn = DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        // The gestion card shows the collaborator's invitation only.
        result.Team.Invitations.Select(invitation => invitation.Email).ShouldBe(["theo@gymxyz.fr"]);
    }

    [Fact]
    public async Task GetGymSettings_ShouldIgnoreAnAcceptedInvitation()
    {
        await using var dbContext = await SeededAsync(nameof(GetGymSettings_ShouldIgnoreAnAcceptedInvitation));

        dbContext.Invitations.Add(new Invitation
        {
            Email = "theo@gymxyz.fr",
            RoleName = GymRoleNames.Coach,
            SentOn = DateTime.UtcNow.AddDays(-3),
            AcceptedOn = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var result = await Handler(dbContext).Handle(new GetGymSettingsQuery(), CancellationToken.None);

        result.Team.Invitations.ShouldBeEmpty();
    }

    // ---- Fixtures -----------------------------------------------------------

    private static async Task<GymDbContext> SeededAsync(string databaseName, Action<Tenant>? customise = null)
    {
        var dbContext = TestInfrastructure.CreateDbContext(databaseName);

        var tenant = new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            Street = "14 rue de la Villette",
            ZipCode = "69003",
            City = "Lyon 3ᵉ",
            Capacity = 180
        };

        customise?.Invoke(tenant);

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        return dbContext;
    }

    private static GetGymSettingsQueryHandler Handler(
        GymDbContext dbContext,
        TestUserDirectory? directory = null,
        string currentUser = "test-user") =>
        new(dbContext,
            new TestTenantContext(TestInfrastructure.DefaultTenantId),
            directory ?? new TestUserDirectory(),
            new TestCurrentUserService { UserName = currentUser });
}
