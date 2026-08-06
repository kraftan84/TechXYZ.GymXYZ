using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// Invariant 6 of the data model: pointing a credit pack debits it <b>once</b>.
/// <para>
/// Both commands that point can run twice over the same seat — a coach tapping
/// the same row again, « Tout présent » pressed after a few seats were already
/// marked — and neither can see what the other did. So the idempotence is tested
/// through the commands themselves rather than through the ledger: a helper that
/// is idempotent in isolation proves nothing about the two callers.
/// </para>
/// </summary>
public class CreditDebitCommandHandlerTests
{
    [Fact]
    public async Task MarkAttendance_ShouldDebitOneEntry_WhenTheMemberCame()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldDebitOneEntry_WhenTheMemberCame));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 2, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);

        pack.CreditsRemaining.ShouldBe(9);
        seat.CreditDebitedFromSubscriptionId.ShouldBe(pack.Id);
    }

    [Fact]
    public async Task MarkAttendance_ShouldNotDebitTwice_WhenTheSameSeatIsPointedAgain()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldNotDebitTwice_WhenTheSameSeatIsPointedAgain));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 2, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        var handler = Mark(dbContext);

        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);
        // Present then late is still one attendance, not two.
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Late), CancellationToken.None);

        pack.CreditsRemaining.ShouldBe(9);
    }

    [Fact]
    public async Task MarkAttendance_ShouldGiveTheEntryBack_WhenTheVerdictIsReversed()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldGiveTheEntryBack_WhenTheVerdictIsReversed));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 2, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        var handler = Mark(dbContext);

        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Absent), CancellationToken.None);

        // A seat corrected to absent did not consume a session.
        pack.CreditsRemaining.ShouldBe(10);
        seat.CreditDebitedFromSubscriptionId.ShouldBeNull();

        // And correcting it back takes exactly one again, not two.
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);
        pack.CreditsRemaining.ShouldBe(9);
    }

    [Fact]
    public async Task MarkAttendance_ShouldNeverRefundPastWhatThePackHeld()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldNeverRefundPastWhatThePackHeld));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 2, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        var handler = Mark(dbContext);

        // Marking absent a seat that never took an entry must not invent one.
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Absent), CancellationToken.None);
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Absent), CancellationToken.None);

        pack.CreditsRemaining.ShouldBe(10);
    }

    [Fact]
    public async Task MarkAttendance_ShouldStillPointASeat_WhenThePackHasRunDry()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldStillPointASeat_WhenThePackHasRunDry));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 2, creditsRemaining: 0);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);

        // Attendance is a fact. Refusing to record it because a pack ran out
        // would hide the very member who needs to renew.
        seat.Status.ShouldBe(AttendanceStatus.Present);
        pack.CreditsRemaining.ShouldBe(0);
        seat.CreditDebitedFromSubscriptionId.ShouldBeNull();
    }

    [Fact]
    public async Task MarkAttendance_ShouldLeaveARecurringPlanAlone()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldLeaveARecurringPlanAlone));

        var session = SeedSheet(dbContext, seats: 2);
        AttachCovers(session, TestPlans.Monthly(), creditsRemaining: null);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);

        // Nothing counts down on an unlimited plan, so nothing is stamped.
        seat.CreditDebitedFromSubscriptionId.ShouldBeNull();
    }

    [Fact]
    public async Task MarkWholeSheet_ShouldDebitEachSeatOnce_EvenAfterSomeWerePointedByHand()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkWholeSheet_ShouldDebitEachSeatOnce_EvenAfterSomeWerePointedByHand));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 3, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        // One seat pointed by hand, then « Tout présent » over the whole sheet,
        // then « Tout présent » again — the case the two commands overlap on.
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(session.Registrations!.First().Id, AttendanceStatus.Present),
            CancellationToken.None);

        var whole = new MarkWholeSheetCommandHandler(dbContext, new MarkWholeSheetCommandValidator());
        await whole.Handle(new MarkWholeSheetCommand(session.Id, AttendanceStatus.Present), CancellationToken.None);
        await whole.Handle(new MarkWholeSheetCommand(session.Id, AttendanceStatus.Present), CancellationToken.None);

        // Each of the three took exactly one entry off their own pack, however
        // many times the button was pressed and whichever command pressed it.
        session.Registrations!.ShouldAllBe(seat => seat.CreditDebitedFromSubscriptionId != null);
        CoversOf(session).ShouldAllBe(cover => cover.CreditsRemaining == 9);
    }

    [Fact]
    public async Task MarkWholeSheet_ShouldGiveEveryEntryBack_WhenTheSheetIsMarkedAbsent()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkWholeSheet_ShouldGiveEveryEntryBack_WhenTheSheetIsMarkedAbsent));
        var (session, pack) = SeedSheetOnAPack(dbContext, seats: 3, creditsRemaining: 10);
        await dbContext.SaveChangesAsync();

        var whole = new MarkWholeSheetCommandHandler(dbContext, new MarkWholeSheetCommandValidator());
        await whole.Handle(new MarkWholeSheetCommand(session.Id, AttendanceStatus.Present), CancellationToken.None);
        CoversOf(session).ShouldAllBe(cover => cover.CreditsRemaining == 9);

        await whole.Handle(new MarkWholeSheetCommand(session.Id, AttendanceStatus.Absent), CancellationToken.None);
        CoversOf(session).ShouldAllBe(cover => cover.CreditsRemaining == 10);
        session.Registrations!.ShouldAllBe(seat => seat.CreditDebitedFromSubscriptionId == null);
    }

    [Fact]
    public async Task MarkAttendance_ShouldReadTheCoverOnTheDayOfTheSession_NotToday()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(MarkAttendance_ShouldReadTheCoverOnTheDayOfTheSession_NotToday));

        var pack = TestPlans.Pack();
        // A sheet from last month, corrected today, against a pack that has
        // since lapsed. The entry belongs to the pack that was running then.
        var session = SeedSheet(dbContext, seats: 1, startsAt: DateTime.Now.AddDays(-40));
        var subscription = AttachCovers(
            session, pack, creditsRemaining: 10, startsInDays: -70, endsInDays: -10)[0];
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);

        subscription.CreditsRemaining.ShouldBe(9);
        seat.CreditDebitedFromSubscriptionId.ShouldBe(subscription.Id);
    }

    private static MarkAttendanceCommandHandler Mark(GymDbContext dbContext) =>
        new(dbContext, new MarkAttendanceCommandValidator());

    private static (Session Session, Subscription Pack) SeedSheetOnAPack(
        GymDbContext dbContext,
        int seats,
        int creditsRemaining)
    {
        var session = SeedSheet(dbContext, seats);
        var packs = AttachCovers(session, TestPlans.Pack(), creditsRemaining);

        // The first seat's own pack. A subscription belongs to one member, so
        // each seat has its own — the tests that care about the others reach
        // them through the member.
        return (session, packs[0]);
    }

    /// <summary>
    /// One cover per member on the sheet, all on the same plan. A subscription
    /// belongs to exactly one member, so a shared pack is not a thing that can
    /// exist — which is precisely why a debit has to be idempotent per seat
    /// rather than per sheet.
    /// </summary>
    private static List<Subscription> AttachCovers(
        Session session,
        Plan plan,
        int? creditsRemaining,
        int startsInDays = -30,
        int endsInDays = 60)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var subscriptions = new List<Subscription>();

        foreach (var seat in session.Registrations!)
        {
            var subscription = new Subscription
            {
                Plan = plan,
                Member = seat.Member,
                StartedOn = today.AddDays(startsInDays),
                EndsOn = today.AddDays(endsInDays),
                CreditsRemaining = creditsRemaining,
                CreditsTotal = plan.IsCredited ? plan.CreditCount : null,
                PriceLabel = plan.FormatPriceLabel()
            };

            seat.Member!.Subscriptions = [subscription];
            subscriptions.Add(subscription);
        }

        return subscriptions;
    }

    private static IEnumerable<Subscription> CoversOf(Session session) =>
        session.Registrations!.Select(seat => seat.Member!.Subscriptions!.Single());

    private static Session SeedSheet(GymDbContext dbContext, int seats, DateTime? startsAt = null)
    {
        var start = startsAt ?? DateTime.Now.AddHours(-1);
        var session = new Session
        {
            CourseTemplate = new CourseTemplate("Power Cycle")
            {
                Discipline = new Discipline("Cycling"),
                Capacity = seats,
                DurationMinutes = 60
            },
            Location = new Location("Studio C") { Kind = LocationKind.Studio, Capacity = 24 },
            StartsAt = start,
            EndsAt = start.AddHours(1),
            Capacity = seats,
            Registrations = [.. Enumerable
                .Range(0, seats)
                .Select(seat => new Registration { Member = new Member($"Member{seat}", "Test") })]
        };

        dbContext.Sessions.Add(session);

        return session;
    }
}
