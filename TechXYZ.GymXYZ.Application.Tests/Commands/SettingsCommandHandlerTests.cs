using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class SettingsCommandHandlerTests
{
    // ---- Identity -----------------------------------------------------------

    [Fact]
    public async Task UpdateGymIdentity_ShouldSaveTheTenantAndCacheTheSchoolZone()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateGymIdentity_ShouldSaveTheTenantAndCacheTheSchoolZone));

        var handler = IdentityHandler(dbContext);

        var saved = await handler.Handle(
            Identity(zipCode: "75001", city: "Paris"), CancellationToken.None);

        saved.ShouldBeTrue();

        var tenant = await dbContext.Tenants.SingleAsync();
        tenant.ZipCode.ShouldBe("75001");
        tenant.City.ShouldBe("Paris");

        // 75 is zone C. The banner reads this rather than recomputing it.
        var settings = await dbContext.GymSettings.SingleAsync();
        settings.SchoolZone.ShouldBe("C");
    }

    [Fact]
    public async Task UpdateGymIdentity_ShouldClearTheAddress_ForACustomerWorkingFromAnArea()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateGymIdentity_ShouldClearTheAddress_ForACustomerWorkingFromAnArea));

        var handler = IdentityHandler(dbContext);

        await handler.Handle(
            Identity(street: "14 rue de la Villette", zipCode: "69003", city: "Lyon 3ᵉ"),
            CancellationToken.None);

        await handler.Handle(
            Identity(street: "14 rue de la Villette", zipCode: "69003", city: "Lyon 3ᵉ",
                areaLabel: "Thonon et alentours"),
            CancellationToken.None);

        var tenant = await dbContext.Tenants.SingleAsync();
        tenant.AreaLabel.ShouldBe("Thonon et alentours");
        tenant.Street.ShouldBeNull();
        tenant.ZipCode.ShouldBeNull();
        tenant.City.ShouldBeNull();

        // No postcode, so no zone to signal holidays for.
        var settings = await dbContext.GymSettings.SingleAsync();
        settings.SchoolZone.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateGymIdentity_ShouldReplaceTheOpeningHours_AndRetireTheRemovedLines()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateGymIdentity_ShouldReplaceTheOpeningHours_AndRetireTheRemovedLines));

        var handler = IdentityHandler(dbContext);

        await handler.Handle(
            Identity(hours:
            [
                new OpeningHoursInput(0, DayOfWeek.Monday, DayOfWeek.Friday, new(6, 30), new(22, 0)),
                new OpeningHoursInput(0, DayOfWeek.Saturday, DayOfWeek.Saturday, new(8, 0), new(19, 0))
            ]),
            CancellationToken.None);

        var stored = await dbContext.OpeningHours.OrderBy(hours => hours.Rank).ToListAsync();
        stored.Count.ShouldBe(2);

        // Saturday goes; Monday–Friday stays and keeps its id.
        await handler.Handle(
            Identity(hours: [new OpeningHoursInput(stored[0].Id, DayOfWeek.Monday, DayOfWeek.Friday, new(7, 0), new(21, 0))]),
            CancellationToken.None);

        var after = await dbContext.OpeningHours.ToListAsync();
        after.Count(hours => hours.IsActive).ShouldBe(1);

        var kept = after.Single(hours => hours.IsActive);
        kept.Id.ShouldBe(stored[0].Id);
        kept.OpensAt.ShouldBe(new TimeOnly(7, 0));

        after.Single(hours => !hours.IsActive).DayFrom.ShouldBe(DayOfWeek.Saturday);
    }

    [Theory]
    [InlineData("6900")]
    [InlineData("6900A")]
    [InlineData("690033")]
    public async Task UpdateGymIdentity_ShouldRefuse_APostcodeThatIsNotFiveDigits(string zipCode)
    {
        await using var dbContext = await SeededAsync($"{nameof(UpdateGymIdentity_ShouldRefuse_APostcodeThatIsNotFiveDigits)}{zipCode}");

        var handler = IdentityHandler(dbContext);

        var act = async () => await handler.Handle(Identity(zipCode: zipCode), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.ZipCodeInvalid);
    }

    [Fact]
    public async Task UpdateGymIdentity_ShouldAcceptAWeekendRange()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateGymIdentity_ShouldAcceptAWeekendRange));

        var handler = IdentityHandler(dbContext);

        // Saturday to Sunday reads backwards on DayOfWeek, where Sunday is zero.
        var saved = await handler.Handle(
            Identity(hours: [new OpeningHoursInput(0, DayOfWeek.Saturday, DayOfWeek.Sunday, new(9, 0), new(13, 0))]),
            CancellationToken.None);

        saved.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateGymIdentity_ShouldRefuse_AClosingTimeBeforeOpening()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateGymIdentity_ShouldRefuse_AClosingTimeBeforeOpening));

        var handler = IdentityHandler(dbContext);

        var act = async () => await handler.Handle(
            Identity(hours: [new OpeningHoursInput(0, DayOfWeek.Monday, DayOfWeek.Monday, new(22, 0), new(6, 0))]),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.ClosingBeforeOpening);
    }

    // ---- Payment methods ----------------------------------------------------

    [Fact]
    public async Task UpdatePaymentMethods_ShouldCreateTheSettingsRow_OnTheFirstSave()
    {
        await using var dbContext = await SeededAsync(nameof(UpdatePaymentMethods_ShouldCreateTheSettingsRow_OnTheFirstSave));

        var handler = new UpdatePaymentMethodsCommandHandler(
            dbContext, new UpdatePaymentMethodsCommandValidator());

        var saved = await handler.Handle(
            new UpdatePaymentMethodsCommand("CHF", "Exonéré", [PaymentMethod.Cash, PaymentMethod.Card]),
            CancellationToken.None);

        saved.ShouldBeTrue();

        var settings = await dbContext.GymSettings.SingleAsync();
        settings.Currency.ShouldBe("CHF");
        settings.VatMention.ShouldBe("Exonéré");
        settings.AcceptedPaymentMethods.ShouldBe([PaymentMethod.Cash, PaymentMethod.Card], ignoreOrder: true);
    }

    [Fact]
    public async Task UpdatePaymentMethods_ShouldRefuse_WhenEveryMethodIsTurnedOff()
    {
        await using var dbContext = await SeededAsync(nameof(UpdatePaymentMethods_ShouldRefuse_WhenEveryMethodIsTurnedOff));

        var handler = new UpdatePaymentMethodsCommandHandler(
            dbContext, new UpdatePaymentMethodsCommandValidator());

        var act = async () => await handler.Handle(
            new UpdatePaymentMethodsCommand("EUR", null, []), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.NoPaymentMethod);
    }

    // ---- Notifications ------------------------------------------------------

    [Fact]
    public async Task UpdateNotificationSettings_ShouldCreateTheRowsACustomerNeverHad()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateNotificationSettings_ShouldCreateTheRowsACustomerNeverHad));

        var handler = new UpdateNotificationSettingsCommandHandler(
            dbContext, new UpdateNotificationSettingsCommandValidator());

        var saved = await handler.Handle(
            new UpdateNotificationSettingsCommand(
            [
                new NotificationSettingInput(NotificationKey.RenewalReminder, true, NotificationChannels.Email),
                new NotificationSettingInput(NotificationKey.CourseCancelled, false, NotificationChannels.None)
            ]),
            CancellationToken.None);

        saved.ShouldBeTrue();

        var stored = await dbContext.NotificationSettings.ToListAsync();
        stored.Count.ShouldBe(2);

        var renewal = stored.Single(setting => setting.Key == NotificationKey.RenewalReminder);
        renewal.IsEnabled.ShouldBeTrue();
        renewal.Channels.ShouldBe(NotificationChannels.Email);
        renewal.Group.ShouldBe(NotificationGroup.MembersAndSubscriptions);

        stored.Single(setting => setting.Key == NotificationKey.CourseCancelled).IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateNotificationSettings_ShouldRefuse_AMessageSwitchedOnWithNoChannel()
    {
        await using var dbContext = await SeededAsync(nameof(UpdateNotificationSettings_ShouldRefuse_AMessageSwitchedOnWithNoChannel));

        var handler = new UpdateNotificationSettingsCommandHandler(
            dbContext, new UpdateNotificationSettingsCommandValidator());

        var act = async () => await handler.Handle(
            new UpdateNotificationSettingsCommand(
                [new NotificationSettingInput(NotificationKey.SeatFreed, true, NotificationChannels.None)]),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.ChannelRequired);
    }

    // ---- Invitations --------------------------------------------------------

    [Fact]
    public async Task InviteTeamMember_ShouldRecordTheInvitation_WithoutCreatingAnAccount()
    {
        await using var dbContext = await SeededAsync(nameof(InviteTeamMember_ShouldRecordTheInvitation_WithoutCreatingAnAccount));
        var directory = TestUserDirectory.WithManager();

        var handler = new InviteTeamMemberCommandHandler(
            dbContext, directory, new InviteTeamMemberCommandValidator());

        var invited = await handler.Handle(
            new InviteTeamMemberCommand("theo.garnier@gymxyz.fr", GymRoleNames.Coach),
            CancellationToken.None);

        invited.ShouldBeTrue();
        directory.Created.ShouldBeEmpty();

        var invitation = await dbContext.Invitations.SingleAsync();
        invitation.Email.ShouldBe("theo.garnier@gymxyz.fr");
        invitation.IsPending.ShouldBeTrue();
        invitation.MemberId.ShouldBeNull();
    }

    [Fact]
    public async Task InviteTeamMember_ShouldRefuse_WhenTheAddressAlreadySignsIn()
    {
        await using var dbContext = await SeededAsync(nameof(InviteTeamMember_ShouldRefuse_WhenTheAddressAlreadySignsIn));
        var directory = TestUserDirectory.WithManager().Add("nora", "nora@gymxyz.fr", GymRoleNames.Coach);

        var handler = new InviteTeamMemberCommandHandler(
            dbContext, directory, new InviteTeamMemberCommandValidator());

        var act = async () => await handler.Handle(
            new InviteTeamMemberCommand("nora@gymxyz.fr", GymRoleNames.Coach), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.AccountAlreadyExists);
    }

    [Fact]
    public async Task InviteTeamMember_ShouldRefuse_ASecondPendingInvitationForTheSameAddress()
    {
        await using var dbContext = await SeededAsync(nameof(InviteTeamMember_ShouldRefuse_ASecondPendingInvitationForTheSameAddress));
        var directory = TestUserDirectory.WithManager();

        var handler = new InviteTeamMemberCommandHandler(
            dbContext, directory, new InviteTeamMemberCommandValidator());

        await handler.Handle(
            new InviteTeamMemberCommand("theo@gymxyz.fr", GymRoleNames.Coach), CancellationToken.None);

        var act = async () => await handler.Handle(
            new InviteTeamMemberCommand("theo@gymxyz.fr", GymRoleNames.Coach), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.InvitationAlreadySent);
    }

    [Fact]
    public async Task InviteTeamMember_ShouldRefuse_ARoleACustomerMayNotAssign()
    {
        await using var dbContext = await SeededAsync(nameof(InviteTeamMember_ShouldRefuse_ARoleACustomerMayNotAssign));

        var handler = new InviteTeamMemberCommandHandler(
            dbContext, TestUserDirectory.WithManager(), new InviteTeamMemberCommandValidator());

        var act = async () => await handler.Handle(
            new InviteTeamMemberCommand("someone@techxyz.fr", GymRoleNames.PlatformAdmin),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.RoleNotAssignable);
    }

    // ---- Access -------------------------------------------------------------

    [Fact]
    public async Task UpdateTeamMemberAccess_ShouldChangeTheRole()
    {
        var directory = TestUserDirectory.WithManager().Add("nora", "nora@gymxyz.fr", GymRoleNames.Coach);

        var handler = new UpdateTeamMemberAccessCommandHandler(
            directory, new UpdateTeamMemberAccessCommandValidator());

        var updated = await handler.Handle(
            new UpdateTeamMemberAccessCommand("nora", GymRoleNames.GymManager), CancellationToken.None);

        updated.ShouldBeTrue();
        directory.RoleChanges.ShouldContain(("nora", GymRoleNames.GymManager));
    }

    [Fact]
    public async Task UpdateTeamMemberAccess_ShouldRefuse_DemotingTheLastManager()
    {
        var directory = TestUserDirectory.WithManager();

        var handler = new UpdateTeamMemberAccessCommandHandler(
            directory, new UpdateTeamMemberAccessCommandValidator());

        var act = async () => await handler.Handle(
            new UpdateTeamMemberAccessCommand("manager", GymRoleNames.Coach), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.LastManagerStands);
    }

    [Fact]
    public async Task RevokeAccess_ShouldCloseTheAccount()
    {
        var directory = TestUserDirectory.WithManager().Add("nora", "nora@gymxyz.fr", GymRoleNames.Coach);

        var handler = new RevokeAccessCommandHandler(
            directory, new TestCurrentUserService { UserName = "test-user" }, new RevokeAccessCommandValidator());

        var revoked = await handler.Handle(new RevokeAccessCommand("nora"), CancellationToken.None);

        revoked.ShouldBeTrue();
        directory.Revoked.ShouldBe(["nora"]);
    }

    [Fact]
    public async Task RevokeAccess_ShouldRefuse_LockingYourselfOut()
    {
        var directory = TestUserDirectory.WithManager("dwayne@gymxyz.fr")
            .Add("other", "nora@gymxyz.fr", GymRoleNames.GymManager);

        var handler = new RevokeAccessCommandHandler(
            directory, new TestCurrentUserService { UserName = "dwayne@gymxyz.fr" }, new RevokeAccessCommandValidator());

        var act = async () => await handler.Handle(new RevokeAccessCommand("manager"), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.CannotRevokeSelf);
        directory.Revoked.ShouldBeEmpty();
    }

    [Fact]
    public async Task RevokeAccess_ShouldRefuse_TakingTheLastManagerOut()
    {
        var directory = TestUserDirectory.WithManager("dwayne@gymxyz.fr");

        var handler = new RevokeAccessCommandHandler(
            directory, new TestCurrentUserService { UserName = "nora@gymxyz.fr" }, new RevokeAccessCommandValidator());

        var act = async () => await handler.Handle(new RevokeAccessCommand("manager"), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Errors.ShouldContain(failure => failure.ErrorMessage == SettingsRules.LastManagerStands);
    }

    // ---- Fixtures -----------------------------------------------------------

    private static async Task<GymDbContext> SeededAsync(string databaseName)
    {
        var dbContext = TestInfrastructure.CreateDbContext(databaseName);

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz"));
        await dbContext.SaveChangesAsync();

        return dbContext;
    }

    private static UpdateGymIdentityCommandHandler IdentityHandler(GymDbContext dbContext) =>
        new(dbContext,
            new TestTenantContext(TestInfrastructure.DefaultTenantId),
            new UpdateGymIdentityCommandValidator());

    private static UpdateGymIdentityCommand Identity(
        string? street = null,
        string? zipCode = null,
        string? city = null,
        string? areaLabel = null,
        IReadOnlyList<OpeningHoursInput>? hours = null) =>
        new("GymXYZ", "Salle de sport & coaching", 180, "901 234 567 00018",
            street, zipCode, city, areaLabel, "contact@gymxyz.fr", "04 78 12 34 56", hours);
}
