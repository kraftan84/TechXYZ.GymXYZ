using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The four attendance commands, and the lock they all share. A validated sheet
/// is read-only for everybody, and only a manager can lift that — which is the
/// decision <c>01-LOTS.md</c> leaves open, answered in the handler because the
/// handler is the only place a caller cannot go around.
/// </summary>
public class AttendanceCommandHandlerTests
{
    [Fact]
    public async Task MarkAttendance_ShouldRecordAnArrivalTimeForSomebodyWhoCame()
    {
        await using var dbContext = CreateDbContext(nameof(MarkAttendance_ShouldRecordAnArrivalTimeForSomebodyWhoCame));
        var session = SeedSession(dbContext, seats: 3, startsAt: DateTime.Now.AddHours(-1));
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Late), CancellationToken.None);

        seat.Status.ShouldBe(AttendanceStatus.Late);
        seat.CheckedInAt.ShouldNotBeNull();
    }

    /// <summary>
    /// Correcting a mistake has to clear the arrival time as well as the verdict:
    /// "dernière venue" reads it, and a stale one would show the member turning
    /// up on a day they were marked away.
    /// </summary>
    [Fact]
    public async Task MarkAttendance_ShouldClearTheArrivalTimeWhenTheVerdictIsReversed()
    {
        await using var dbContext = CreateDbContext(nameof(MarkAttendance_ShouldClearTheArrivalTimeWhenTheVerdictIsReversed));
        var session = SeedSession(dbContext, seats: 3, startsAt: DateTime.Now.AddHours(-1));
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();
        var handler = Mark(dbContext);

        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);
        await handler.Handle(new MarkAttendanceCommand(seat.Id, AttendanceStatus.Absent), CancellationToken.None);

        seat.Status.ShouldBe(AttendanceStatus.Absent);
        seat.CheckedInAt.ShouldBeNull();
    }

    [Fact]
    public async Task MarkAttendance_ShouldRefuseAValidatedSheet_AndSayWhy()
    {
        await using var dbContext = CreateDbContext(nameof(MarkAttendance_ShouldRefuseAValidatedSheet_AndSayWhy));
        var session = SeedSession(dbContext, seats: 3, startsAt: DateTime.Now.AddHours(-2));
        session.AttendanceClosedAt = DateTime.Now.AddHours(-1);
        await dbContext.SaveChangesAsync();

        var seat = session.Registrations!.First();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Mark(dbContext).Handle(
                new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None));

        // The toast is built from Errors, not from the exception message.
        error.Errors.ShouldNotBeEmpty();
        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.SheetClosed);
        seat.Status.ShouldBe(AttendanceStatus.Pending);
    }

    [Fact]
    public async Task MarkAttendance_ShouldRefuseACancelledSession()
    {
        await using var dbContext = CreateDbContext(nameof(MarkAttendance_ShouldRefuseACancelledSession));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-1));
        session.Status = SessionStatus.Cancelled;
        await dbContext.SaveChangesAsync();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Mark(dbContext).Handle(
                new MarkAttendanceCommand(session.Registrations!.First().Id, AttendanceStatus.Present),
                CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.SessionCancelled);
    }

    /// <summary>
    /// « Tout présent » fills the sheet — but not the queue behind it. A waiting
    /// seat never got the member into the room, so there is nothing to point.
    /// </summary>
    [Fact]
    public async Task MarkWholeSheet_ShouldFillEverySeatButTheWaitingList()
    {
        await using var dbContext = CreateDbContext(nameof(MarkWholeSheet_ShouldFillEverySeatButTheWaitingList));
        var session = SeedSession(dbContext, seats: 3, startsAt: DateTime.Now.AddHours(-1), waitlisted: 2);
        await dbContext.SaveChangesAsync();

        await new MarkWholeSheetCommandHandler(dbContext, new MarkWholeSheetCommandValidator())
            .Handle(new MarkWholeSheetCommand(session.Id, AttendanceStatus.Present), CancellationToken.None);

        session.Registrations!.Count(seat => seat.Status == AttendanceStatus.Present).ShouldBe(3);
        session.Registrations!.Count(seat => seat.Status == AttendanceStatus.Pending).ShouldBe(2);
        session.Registrations!.Where(seat => seat.IsWaitlisted)
            .ShouldAllBe(seat => seat.Status == AttendanceStatus.Pending);
    }

    [Fact]
    public async Task CloseAttendanceSheet_ShouldLockIt()
    {
        await using var dbContext = CreateDbContext(nameof(CloseAttendanceSheet_ShouldLockIt));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-1));
        await dbContext.SaveChangesAsync();

        await Close(dbContext).Handle(new CloseAttendanceSheetCommand(session.Id), CancellationToken.None);

        session.AttendanceClosedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CloseAttendanceSheet_ShouldRefuseASessionThatHasNotStarted()
    {
        await using var dbContext = CreateDbContext(nameof(CloseAttendanceSheet_ShouldRefuseASessionThatHasNotStarted));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddDays(1));
        await dbContext.SaveChangesAsync();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Close(dbContext).Handle(new CloseAttendanceSheetCommand(session.Id), CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.SessionNotStarted);
        session.AttendanceClosedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Reopen_ShouldRefuseAnybodyButAManager()
    {
        await using var dbContext = CreateDbContext(nameof(Reopen_ShouldRefuseAnybodyButAManager));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-2));
        session.AttendanceClosedAt = DateTime.Now.AddHours(-1);
        await dbContext.SaveChangesAsync();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Reopen(dbContext, GymRoleNames.Coach)
                .Handle(new ReopenAttendanceSheetCommand(session.Id), CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.ReopenReserved);
        session.AttendanceClosedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Reopen_ShouldUnlockTheSheetForAManager_AndLeaveATrace()
    {
        await using var dbContext = CreateDbContext(nameof(Reopen_ShouldUnlockTheSheetForAManager_AndLeaveATrace));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-2));
        session.AttendanceClosedAt = DateTime.Now.AddHours(-1);
        await dbContext.SaveChangesAsync();

        await Reopen(dbContext, GymRoleNames.GymManager)
            .Handle(new ReopenAttendanceSheetCommand(session.Id), CancellationToken.None);

        session.AttendanceClosedAt.ShouldBeNull();
        session.AttendanceReopenedBy.ShouldBe("the-manager");
        session.AttendanceReopenedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Reopen_ShouldRefuseASheetThatWasNeverValidated()
    {
        await using var dbContext = CreateDbContext(nameof(Reopen_ShouldRefuseASheetThatWasNeverValidated));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-1));
        await dbContext.SaveChangesAsync();

        var error = await Should.ThrowAsync<ValidationException>(async () =>
            await Reopen(dbContext, GymRoleNames.GymManager)
                .Handle(new ReopenAttendanceSheetCommand(session.Id), CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.SheetNotClosed);
    }

    /// <summary>
    /// Once reopened, the sheet takes writes again — the lock is the only thing
    /// that was standing in the way.
    /// </summary>
    [Fact]
    public async Task AReopenedSheet_ShouldAcceptCorrections()
    {
        await using var dbContext = CreateDbContext(nameof(AReopenedSheet_ShouldAcceptCorrections));
        var session = SeedSession(dbContext, seats: 2, startsAt: DateTime.Now.AddHours(-2));
        session.AttendanceClosedAt = DateTime.Now.AddHours(-1);
        await dbContext.SaveChangesAsync();

        await Reopen(dbContext, GymRoleNames.GymManager)
            .Handle(new ReopenAttendanceSheetCommand(session.Id), CancellationToken.None);

        var seat = session.Registrations!.First();
        await Mark(dbContext).Handle(
            new MarkAttendanceCommand(seat.Id, AttendanceStatus.Present), CancellationToken.None);

        seat.Status.ShouldBe(AttendanceStatus.Present);
    }

    private static GymDbContext CreateDbContext(string name) => TestInfrastructure.CreateDbContext(name);

    private static MarkAttendanceCommandHandler Mark(GymDbContext dbContext) =>
        new(dbContext, new MarkAttendanceCommandValidator());

    private static CloseAttendanceSheetCommandHandler Close(GymDbContext dbContext) =>
        new(dbContext, new CloseAttendanceSheetCommandValidator());

    private static ReopenAttendanceSheetCommandHandler Reopen(GymDbContext dbContext, params string[] roles)
    {
        ICurrentUserService user = new TestCurrentUserService(roles) { UserName = "the-manager" };

        return new ReopenAttendanceSheetCommandHandler(
            dbContext, new ReopenAttendanceSheetCommandValidator(), user);
    }

    private static Session SeedSession(
        GymDbContext dbContext,
        int seats,
        DateTime startsAt,
        int waitlisted = 0)
    {
        var session = new Session
        {
            CourseTemplate = new CourseTemplate("Power Cycle")
            {
                Discipline = new Discipline("Cycling"),
                Capacity = seats,
                DurationMinutes = 60
            },
            Location = new Location("Studio C") { Kind = LocationKind.Studio, Capacity = 24 },
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            Capacity = seats,
            Registrations = [.. Enumerable
                .Range(0, seats + waitlisted)
                .Select(seat => new Registration
                {
                    Member = new Member($"Member{seat}", "Test"),
                    IsWaitlisted = seat >= seats
                })]
        };

        dbContext.Sessions.Add(session);

        return session;
    }
}
