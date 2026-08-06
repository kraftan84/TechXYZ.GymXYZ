using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Recording money that already changed hands, and chasing the money that has
/// not. Neither takes a payment or sends a message: there is no provider in this
/// lot, and the channel arrives with the Réglages.
/// </summary>
public class PaymentCommandHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task Record_ShouldLabelThePaymentWithThePlanItPaidFor()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Record_ShouldLabelThePaymentWithThePlanItPaidFor));
        var cover = Seed(dbContext);
        await dbContext.SaveChangesAsync();

        var id = await Record(dbContext).Handle(
            new RecordPaymentCommand(cover.MemberId, cover.Id, 49m, PaymentMethod.Card, Today),
            CancellationToken.None);

        var payment = dbContext.Payments.Single(candidate => candidate.Id == id);
        payment.Label.ShouldBe("Illimité mensuel");
        payment.Status.ShouldBe(PaymentStatus.Collected);
        payment.MemberId.ShouldBe(cover.MemberId);
    }

    [Fact]
    public async Task Record_ShouldAcceptARejectedDirectDebit()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Record_ShouldAcceptARejectedDirectDebit));
        var cover = Seed(dbContext);
        await dbContext.SaveChangesAsync();

        var id = await Record(dbContext).Handle(
            new RecordPaymentCommand(cover.MemberId, cover.Id, 49m, PaymentMethod.SepaDirectDebit,
                Today.AddDays(-1), PaymentStatus.Rejected),
            CancellationToken.None);

        // A rejection is exactly the row somebody opens this screen to enter,
        // which is why the status is an input rather than always "Encaissé".
        dbContext.Payments.Single(candidate => candidate.Id == id)
            .Status.ShouldBe(PaymentStatus.Rejected);
    }

    [Fact]
    public async Task Record_ShouldRefuseACoverBelongingToSomebodyElse()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Record_ShouldRefuseACoverBelongingToSomebodyElse));
        var cover = Seed(dbContext);
        var stranger = new Member("Théo", "Garnier");
        dbContext.Members.Add(stranger);
        await dbContext.SaveChangesAsync();

        // Attaching a payment to somebody else's cover would move money between
        // two accounts and make one of them read paid up.
        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Record(dbContext).Handle(
                new RecordPaymentCommand(stranger.Id, cover.Id, 49m, PaymentMethod.Cash, Today),
                CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(PaymentRules.SubscriptionNotOwned);
    }

    [Fact]
    public async Task Record_ShouldRefuseADateInTheFuture()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Record_ShouldRefuseADateInTheFuture));
        var cover = Seed(dbContext);
        await dbContext.SaveChangesAsync();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Record(dbContext).Handle(
                new RecordPaymentCommand(cover.MemberId, cover.Id, 49m, PaymentMethod.Card,
                    Today.AddDays(1)),
                CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(PaymentRules.DateInTheFuture);
    }

    [Fact]
    public async Task Reminder_ShouldStampTheChaseWithoutSendingAnything()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Reminder_ShouldStampTheChaseWithoutSendingAnything));

        // A lapsed cover with a rejected debit behind it — "En retard".
        var cover = Seed(dbContext, startsInDays: -60, endsInDays: -5);
        cover.Payments =
        [
            new Payment
            {
                Member = cover.Member,
                Date = Today.AddDays(-6),
                Label = "Illimité mensuel",
                Amount = 49m,
                Method = PaymentMethod.SepaDirectDebit,
                Status = PaymentStatus.Rejected
            }
        ];
        await dbContext.SaveChangesAsync();

        var sent = await Remind(dbContext).Handle(
            new SendPaymentReminderCommand(cover.Id), CancellationToken.None);

        sent.IsSaved.ShouldBeTrue();

        // The stamp is what stops a second chase going out minutes later.
        cover.LastReminderSentOn.ShouldBe(Today);
    }

    [Fact]
    public async Task Reminder_ShouldRefuseACoverThatIsUpToDate()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Reminder_ShouldRefuseACoverThatIsUpToDate));
        var cover = Seed(dbContext, startsInDays: -10, endsInDays: 40);
        await dbContext.SaveChangesAsync();

        // Chasing somebody paid up and covered for another month is how a gym
        // trains its members to ignore its e-mails.
        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Remind(dbContext).Handle(
                new SendPaymentReminderCommand(cover.Id), CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(PaymentRules.NothingToChase);
        cover.LastReminderSentOn.ShouldBeNull();
    }

    [Fact]
    public async Task Reminder_ShouldChaseACoverAboutToExpire()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Reminder_ShouldChaseACoverAboutToExpire));
        var cover = Seed(dbContext, startsInDays: -25, endsInDays: 3);
        await dbContext.SaveChangesAsync();

        (await Remind(dbContext).Handle(new SendPaymentReminderCommand(cover.Id), CancellationToken.None))
            .IsSaved.ShouldBeTrue();

        cover.LastReminderSentOn.ShouldBe(Today);
    }

    [Fact]
    public async Task Reminder_ShouldWriteToTheMember()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Reminder_ShouldWriteToTheMember));
        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz") { Email = "contact@gymxyz.fr" });
        var cover = Seed(dbContext, startsInDays: -25, endsInDays: 3);
        await dbContext.SaveChangesAsync();

        var sender = new TestEmailSender();
        var outcome = await Remind(dbContext, sender).Handle(
            new SendPaymentReminderCommand(cover.Id), CancellationToken.None);

        outcome.Sent.ShouldBe(1);
        sender.Single.ToAddress.ShouldBe("laetitia@gymxyz.fr");
        sender.Single.TextBody.ShouldContain("Laetitia");
        sender.Single.ReplyToAddress.ShouldBe("contact@gymxyz.fr");
    }

    [Fact]
    public async Task Reminder_ShouldKeepTheStamp_WhenTheGymTurnedRelancesOff()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Reminder_ShouldKeepTheStamp_WhenTheGymTurnedRelancesOff));
        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz"));
        dbContext.NotificationSettings.Add(new NotificationSetting
        {
            Group = NotificationGroup.MembersAndSubscriptions,
            Key = NotificationKey.RenewalReminder,
            IsEnabled = false,
            Channels = NotificationChannels.Email
        });
        var cover = Seed(dbContext, startsInDays: -25, endsInDays: 3);
        await dbContext.SaveChangesAsync();

        var sender = new TestEmailSender();
        var outcome = await Remind(dbContext, sender).Handle(
            new SendPaymentReminderCommand(cover.Id), CancellationToken.None);

        // Suppressed is not a failure: the silence was asked for, and the screen
        // says so rather than reporting an error nobody caused.
        outcome.WasSuppressed.ShouldBeTrue();
        outcome.HasFailures.ShouldBeFalse();
        sender.Sent.ShouldBeEmpty();
        cover.LastReminderSentOn.ShouldBe(Today);
    }

    [Fact]
    public async Task Reminder_ShouldKeepTheStamp_WhenTheSendFails()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Reminder_ShouldKeepTheStamp_WhenTheSendFails));
        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz"));
        var cover = Seed(dbContext, startsInDays: -25, endsInDays: 3);
        await dbContext.SaveChangesAsync();

        var outcome = await Remind(dbContext, new TestEmailSender(fails: true)).Handle(
            new SendPaymentReminderCommand(cover.Id), CancellationToken.None);

        // The decision to chase survives the channel: losing the stamp would
        // have somebody send a second relance minutes later.
        outcome.HasFailures.ShouldBeTrue();
        cover.LastReminderSentOn.ShouldBe(Today);
    }

    private static RecordPaymentCommandHandler Record(GymDbContext dbContext) =>
        new(dbContext, new RecordPaymentCommandValidator());

    private static SendPaymentReminderCommandHandler Remind(
        GymDbContext dbContext,
        TestEmailSender? emailSender = null) =>
        new(dbContext,
            emailSender ?? new TestEmailSender(),
            new TestTenantContext(TestInfrastructure.DefaultTenantId),
            new SendPaymentReminderCommandValidator());

    private static Subscription Seed(
        GymDbContext dbContext,
        int startsInDays = -10,
        int endsInDays = 20)
    {
        var plan = TestPlans.Monthly();
        var subscription = new Subscription
        {
            Member = new Member("Laetitia", "Moriceau") { Email = "laetitia@gymxyz.fr" },
            Plan = plan,
            StartedOn = Today.AddDays(startsInDays),
            EndsOn = Today.AddDays(endsInDays),
            PriceLabel = plan.FormatPriceLabel(),
            Price = plan.Price,
            MonthlyPrice = SubscriptionFactory.MonthlyPriceOf(plan)
        };

        dbContext.Plans.Add(plan);
        dbContext.Subscriptions.Add(subscription);

        return subscription;
    }
}
