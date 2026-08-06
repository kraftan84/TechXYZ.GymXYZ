using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// What the three sends of lot 8 owe: the right message to the right person, the
/// gym's switches consulted first, and the write standing whatever the channel
/// does.
/// </summary>
public class NotificationSendingTests
{
    // ---- The gate -----------------------------------------------------------

    [Fact]
    public async Task Policy_ShouldFallBackToTheDefault_WhenTheCustomerHasNoRow()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Policy_ShouldFallBackToTheDefault_WhenTheCustomerHasNoRow));

        // A gym created before a message existed never chose to silence it.
        (await NotificationPolicy.AllowsEmailAsync(dbContext, NotificationKey.CourseCancelled))
            .ShouldBeTrue();

        // NewRegistration is the one the hand-off draws switched off.
        (await NotificationPolicy.AllowsEmailAsync(dbContext, NotificationKey.NewRegistration))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task Policy_ShouldObeyAStoredSwitch()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Policy_ShouldObeyAStoredSwitch));

        dbContext.NotificationSettings.Add(new NotificationSetting
        {
            Group = NotificationGroup.CoursesAndAttendance,
            Key = NotificationKey.CourseCancelled,
            IsEnabled = false,
            Channels = NotificationChannels.Email
        });
        await dbContext.SaveChangesAsync();

        (await NotificationPolicy.AllowsEmailAsync(dbContext, NotificationKey.CourseCancelled))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task Policy_ShouldRefuseAChannelThatIsNotTicked()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Policy_ShouldRefuseAChannelThatIsNotTicked));

        dbContext.NotificationSettings.Add(new NotificationSetting
        {
            Group = NotificationGroup.CoursesAndAttendance,
            Key = NotificationKey.CourseCancelled,
            IsEnabled = true,
            Channels = NotificationChannels.Sms
        });
        await dbContext.SaveChangesAsync();

        (await NotificationPolicy.AllowsEmailAsync(dbContext, NotificationKey.CourseCancelled))
            .ShouldBeFalse();
    }

    // ---- The absence chase --------------------------------------------------

    [Fact]
    public async Task AbsenceChase_ShouldWriteToEveryMemberAsked()
    {
        await using var dbContext = await SeededAsync(nameof(AbsenceChase_ShouldWriteToEveryMemberAsked));

        var first = new Member("Théo", "Garnier") { Email = "theo@gymxyz.fr" };
        var second = new Member("Camille", "Durand") { Email = "camille@gymxyz.fr" };
        dbContext.Members.AddRange(first, second);
        await dbContext.SaveChangesAsync();

        var sender = new TestEmailSender();

        var outcome = await Chase(dbContext, sender).Handle(
            new SendAbsenceReminderCommand([first.Id, second.Id]), CancellationToken.None);

        outcome.Sent.ShouldBe(2);
        outcome.Failed.ShouldBe(0);
        sender.Sent.Select(message => message.ToAddress)
            .ShouldBe(["theo@gymxyz.fr", "camille@gymxyz.fr"], ignoreOrder: true);
    }

    [Fact]
    public async Task AbsenceChase_ShouldSendFromTheGymAndReplyToIt()
    {
        await using var dbContext = await SeededAsync(nameof(AbsenceChase_ShouldSendFromTheGymAndReplyToIt));

        var member = new Member("Théo", "Garnier") { Email = "theo@gymxyz.fr" };
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var sender = new TestEmailSender();
        await Chase(dbContext, sender).Handle(
            new SendAbsenceReminderCommand(member.Id), CancellationToken.None);

        // The gym's name leads and a reply reaches the gym; the address it is
        // dispatched from belongs to the service and is the sender's business.
        sender.Single.FromName.ShouldBe("GymXYZ");
        sender.Single.ReplyToAddress.ShouldBe("contact@gymxyz.fr");
        sender.Single.TextBody.ShouldContain("Théo");
        sender.Single.Subject.ShouldContain("GymXYZ");
    }

    [Fact]
    public async Task AbsenceChase_ShouldSkipAMemberWithNoAddress()
    {
        await using var dbContext = await SeededAsync(nameof(AbsenceChase_ShouldSkipAMemberWithNoAddress));

        var withAddress = new Member("Théo", "Garnier") { Email = "theo@gymxyz.fr" };
        var without = new Member("Sans", "Adresse");
        dbContext.Members.AddRange(withAddress, without);
        await dbContext.SaveChangesAsync();

        var sender = new TestEmailSender();

        var outcome = await Chase(dbContext, sender).Handle(
            new SendAbsenceReminderCommand([withAddress.Id, without.Id]), CancellationToken.None);

        outcome.Sent.ShouldBe(1);
        sender.Sent.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AbsenceChase_ShouldReportNotFound_WhenNobodyCanBeWrittenTo()
    {
        await using var dbContext = await SeededAsync(nameof(AbsenceChase_ShouldReportNotFound_WhenNobodyCanBeWrittenTo));

        var outcome = await Chase(dbContext, new TestEmailSender()).Handle(
            new SendAbsenceReminderCommand(4321), CancellationToken.None);

        outcome.IsSaved.ShouldBeFalse();
    }

    [Fact]
    public async Task AbsenceChase_ShouldCountAFailedSend()
    {
        await using var dbContext = await SeededAsync(nameof(AbsenceChase_ShouldCountAFailedSend));

        var member = new Member("Théo", "Garnier") { Email = "theo@gymxyz.fr" };
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var outcome = await Chase(dbContext, new TestEmailSender(fails: true)).Handle(
            new SendAbsenceReminderCommand(member.Id), CancellationToken.None);

        outcome.IsSaved.ShouldBeTrue();
        outcome.Sent.ShouldBe(0);
        outcome.Failed.ShouldBe(1);
        outcome.HasFailures.ShouldBeTrue();
    }

    // ---- Fixtures -----------------------------------------------------------

    private static async Task<GymDbContext> SeededAsync(string databaseName)
    {
        var dbContext = TestInfrastructure.CreateDbContext(databaseName);

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            Email = "contact@gymxyz.fr"
        });
        await dbContext.SaveChangesAsync();

        return dbContext;
    }

    private static SendAbsenceReminderCommandHandler Chase(GymDbContext dbContext, TestEmailSender sender) =>
        new(dbContext,
            sender,
            new TestTenantContext(TestInfrastructure.DefaultTenantId),
            new SendAbsenceReminderCommandValidator());
}
