using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Cancelling a session warns everybody who held a seat — and stays cancelled
/// whatever the warning does.
/// </summary>
public class CancellationNoticeTests
{
    [Fact]
    public async Task Cancel_ShouldWarnEveryRegisteredMember()
    {
        await using var dbContext = await SeededAsync(nameof(Cancel_ShouldWarnEveryRegisteredMember));
        var session = await dbContext.Sessions.SingleAsync();
        var sender = new TestEmailSender();

        var outcome = await Cancel(dbContext, sender).Handle(
            new CancelSessionCommand(session.Id, "Coach malade"), CancellationToken.None);

        outcome.IsSaved.ShouldBeTrue();
        outcome.Sent.ShouldBe(2);

        var message = sender.Sent.First(candidate => candidate.ToAddress == "theo@gymxyz.fr");
        message.Subject.ShouldContain("Pilates");
        message.TextBody.ShouldContain("Pilates");
        message.TextBody.ShouldContain("Coach malade");
        message.ReplyToAddress.ShouldBe("contact@gymxyz.fr");
    }

    [Fact]
    public async Task Cancel_ShouldStand_WhenTheWarningsFail()
    {
        await using var dbContext = await SeededAsync(nameof(Cancel_ShouldStand_WhenTheWarningsFail));
        var session = await dbContext.Sessions.SingleAsync();

        var outcome = await Cancel(dbContext, new TestEmailSender(fails: true)).Handle(
            new CancelSessionCommand(session.Id), CancellationToken.None);

        // The one that matters: a session that is off does not come back on
        // because a mail server was unreachable.
        outcome.IsSaved.ShouldBeTrue();
        outcome.Failed.ShouldBe(2);

        (await dbContext.Sessions.SingleAsync()).Status.ShouldBe(SessionStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_ShouldSendNothing_WhenTheGymTurnedTheNoticeOff()
    {
        await using var dbContext = await SeededAsync(nameof(Cancel_ShouldSendNothing_WhenTheGymTurnedTheNoticeOff));

        dbContext.NotificationSettings.Add(new NotificationSetting
        {
            Group = NotificationGroup.CoursesAndAttendance,
            Key = NotificationKey.CourseCancelled,
            IsEnabled = false,
            Channels = NotificationChannels.Email
        });
        await dbContext.SaveChangesAsync();

        var session = await dbContext.Sessions.SingleAsync();
        var sender = new TestEmailSender();

        var outcome = await Cancel(dbContext, sender).Handle(
            new CancelSessionCommand(session.Id), CancellationToken.None);

        outcome.IsSaved.ShouldBeTrue();
        outcome.WasSuppressed.ShouldBeTrue();
        outcome.HasFailures.ShouldBeFalse();
        sender.Sent.ShouldBeEmpty();

        (await dbContext.Sessions.SingleAsync()).Status.ShouldBe(SessionStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_ShouldSkipASeatWithNoAddress()
    {
        await using var dbContext = await SeededAsync(
            nameof(Cancel_ShouldSkipASeatWithNoAddress), withAddresses: false);

        var session = await dbContext.Sessions.SingleAsync();
        var sender = new TestEmailSender();

        var outcome = await Cancel(dbContext, sender).Handle(
            new CancelSessionCommand(session.Id), CancellationToken.None);

        outcome.IsSaved.ShouldBeTrue();
        outcome.Sent.ShouldBe(0);
        outcome.HasFailures.ShouldBeFalse();
        sender.Sent.ShouldBeEmpty();
    }

    // ---- Fixtures -----------------------------------------------------------

    private static async Task<GymDbContext> SeededAsync(string databaseName, bool withAddresses = true)
    {
        var dbContext = TestInfrastructure.CreateDbContext(databaseName);

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz") { Email = "contact@gymxyz.fr" });

        var discipline = new Discipline("Pilates");
        var location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 20 };
        var template = new CourseTemplate("Pilates matinal")
        {
            Discipline = discipline,
            DefaultLocation = location,
            DurationMinutes = 60,
            Capacity = 20
        };

        var session = new Session
        {
            CourseTemplate = template,
            Location = location,
            StartsAt = DateTime.Today.AddDays(2).AddHours(9),
            EndsAt = DateTime.Today.AddDays(2).AddHours(10),
            Capacity = 20
        };

        var theo = new Member("Théo", "Garnier") { Email = withAddresses ? "theo@gymxyz.fr" : null };
        var camille = new Member("Camille", "Durand") { Email = withAddresses ? "camille@gymxyz.fr" : null };

        dbContext.Members.AddRange(theo, camille);
        dbContext.Sessions.Add(session);
        dbContext.Registrations.AddRange(
            new Registration { Session = session, Member = theo },
            new Registration { Session = session, Member = camille });

        await dbContext.SaveChangesAsync();

        return dbContext;
    }

    private static CancelSessionCommandHandler Cancel(GymDbContext dbContext, TestEmailSender sender) =>
        new(dbContext,
            sender,
            new TestTenantContext(TestInfrastructure.DefaultTenantId),
            new CancelSessionCommandValidator(),
            TestCurrentUserService.Manager());
}
