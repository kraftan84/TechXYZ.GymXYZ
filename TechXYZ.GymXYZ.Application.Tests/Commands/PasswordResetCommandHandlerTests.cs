using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The two public commands of the product, and the silence they are built on.
/// <para>
/// Everything asserted here is about what the caller is <em>not</em> told. The
/// screen behind these shows one sentence whatever happens, so the handlers must
/// give it nothing to branch on — not a return value, not an exception, not a
/// difference in how long they take to say nothing.
/// </para>
/// </summary>
public class PasswordResetCommandHandlerTests
{
    private const string ResetPageUrl = "https://teamtrainers.gymxyz.fr/account/reinitialisation";

    [Fact]
    public async Task Request_ShouldSendALinkToAnAddressThatHasAnAccount()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Request_ShouldSendALinkToAnAddressThatHasAnAccount));

        await SeedTenantAsync(dbContext);

        var directory = new TestUserDirectory()
            .Add("aurelie", "aurelie@teamtrainers.fr", GymRoleNames.GymManager, displayName: "Aurélie Siquier");
        var emails = new TestEmailSender();

        await Handler(dbContext, directory, emails)
            .Handle(new RequestPasswordResetCommand("aurelie@teamtrainers.fr", ResetPageUrl), default);

        var message = emails.Single;

        message.ToAddress.ShouldBe("aurelie@teamtrainers.fr");

        // Signed by the customer's own space. A reset e-mail from a brand the
        // reader has never heard of is a reset e-mail they report as phishing.
        message.Subject.ShouldStartWith("Team Trainer's");
        message.TextBody.ShouldContain("Bonjour Aurélie Siquier,");
        message.TextBody.ShouldContain(ResetPageUrl);
        message.TextBody.ShouldContain("30 minutes");
    }

    [Fact]
    public async Task Request_ShouldCarryTheTokenOnTheLinkItSends()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Request_ShouldCarryTheTokenOnTheLinkItSends));

        await SeedTenantAsync(dbContext);

        var directory = new TestUserDirectory()
            .Add("aurelie", "aurelie@teamtrainers.fr", GymRoleNames.GymManager);
        var emails = new TestEmailSender();

        await Handler(dbContext, directory, emails)
            .Handle(new RequestPasswordResetCommand("aurelie@teamtrainers.fr", ResetPageUrl), default);

        // Both halves, escaped: the address the link is for and the token that
        // authorises it. A link missing either lands on "ce lien n'est plus
        // valable" and looks like an expiry.
        emails.Single.TextBody.ShouldContain("email=aurelie%40teamtrainers.fr");
        emails.Single.TextBody.ShouldContain("token=token-for-aurelie");
    }

    [Fact]
    public async Task Request_ShouldSendNothingForAnAddressNobodyUses()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Request_ShouldSendNothingForAnAddressNobodyUses));

        await SeedTenantAsync(dbContext);

        var emails = new TestEmailSender();

        await Handler(dbContext, new TestUserDirectory(), emails)
            .Handle(new RequestPasswordResetCommand("personne@nulle-part.fr", ResetPageUrl), default);

        emails.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Request_ShouldNotThrowForAnAddressNobodyUses()
    {
        // The point of the whole design. An exception here would reach the screen
        // as an error page for an unknown address and as a calm confirmation for
        // a known one — which is the enumeration the silence exists to prevent,
        // rebuilt out of stack traces.
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Request_ShouldNotThrowForAnAddressNobodyUses));

        await SeedTenantAsync(dbContext);

        var send = () => Handler(dbContext, new TestUserDirectory(), new TestEmailSender())
            .Handle(new RequestPasswordResetCommand("personne@nulle-part.fr", ResetPageUrl), default);

        await send.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Request_ShouldSendNothingToARevokedAccess()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Request_ShouldSendNothingToARevokedAccess));

        await SeedTenantAsync(dbContext);

        var directory = new TestUserDirectory()
            .Add("marine", "marine@teamtrainers.fr", GymRoleNames.Coach, isRevoked: true);
        var emails = new TestEmailSender();

        await Handler(dbContext, directory, emails)
            .Handle(new RequestPasswordResetCommand("marine@teamtrainers.fr", ResetPageUrl), default);

        emails.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reset_ShouldReportWhatTheDirectoryDecided()
    {
        var directory = new TestUserDirectory()
            .Add("aurelie", "aurelie@teamtrainers.fr", GymRoleNames.GymManager);

        var handler = new ResetPasswordCommandHandler(directory);

        var ok = await handler.Handle(
            new ResetPasswordCommand("aurelie@teamtrainers.fr", "token-for-aurelie", "Nouveau!Mdp2026"), default);
        ok.Succeeded.ShouldBeTrue();

        var wrongToken = await handler.Handle(
            new ResetPasswordCommand("aurelie@teamtrainers.fr", "token-forgé", "Nouveau!Mdp2026"), default);
        wrongToken.LinkNoLongerValid.ShouldBeTrue();

        var tooShort = await handler.Handle(
            new ResetPasswordCommand("aurelie@teamtrainers.fr", "token-for-aurelie", "court1A"), default);
        tooShort.Succeeded.ShouldBeFalse();
        tooShort.LinkNoLongerValid.ShouldBeFalse();
        tooShort.PasswordErrors.ShouldNotBeEmpty();
    }

    private static RequestPasswordResetCommandHandler Handler(
        GymDbContext dbContext,
        TestUserDirectory directory,
        TestEmailSender emails)
        => new(dbContext, directory, emails, new TestTenantContext(TestInfrastructure.DefaultTenantId));

    private static async Task SeedTenantAsync(GymDbContext dbContext)
    {
        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            Id = TestInfrastructure.DefaultTenantId
        });

        await dbContext.SaveChangesAsync();
    }
}
