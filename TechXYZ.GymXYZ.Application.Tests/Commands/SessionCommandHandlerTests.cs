using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class SessionCommandHandlerTests
{
    /// <summary>
    /// The capacity travels with the occurrence. Editing the catalogue afterwards
    /// must not rewrite the history of the sessions already run.
    /// </summary>
    [Fact]
    public async Task Create_ShouldCopyTheCapacityOfTheCourse()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldCopyTheCapacityOfTheCourse));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        var id = await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);

        var session = dbContext.Sessions.Single(candidate => candidate.Id == id);
        session.Capacity.ShouldBe(16);
        session.EndsAt.ShouldBe(session.StartsAt.AddMinutes(60));
        session.Status.ShouldBe(SessionStatus.Scheduled);

        // One occurrence is not a series, and claiming one would be a lie the
        // "this and all the following" scope would then act on.
        session.SeriesId.ShouldBeNull();
    }

    /// <summary>
    /// A recurrence materialises the occurrences and ties them together, which
    /// is what makes editing or cancelling the rest of the term one query.
    /// </summary>
    [Fact]
    public async Task Create_ShouldWriteOneRowPerWeekUnderOneSeries()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldWriteOneRowPerWeekUnderOneSeries));
        var (template, location, coach) = await SeedCatalogueAsync(dbContext);
        var start = NextMonday(9);

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, start, coach.Id, recurrenceWeeks: 4),
            CancellationToken.None);

        var sessions = dbContext.Sessions.OrderBy(session => session.StartsAt).ToList();
        sessions.Count.ShouldBe(4);
        sessions.Select(session => session.SeriesId).Distinct().Count().ShouldBe(1);
        sessions[0].SeriesId.ShouldNotBeNull();
        sessions[3].StartsAt.ShouldBe(start.AddDays(21));
    }

    /// <summary>Invariant 1: a session never seats more than the venue holds.</summary>
    [Fact]
    public async Task Create_ShouldRefuseMorePeopleThanTheVenueHolds()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldRefuseMorePeopleThanTheVenueHolds));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());

        var act = () => handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9), capacity: 40),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Message.ShouldContain("Studio A");
    }

    /// <summary>Invariant 2: two sessions of the same venue never overlap.</summary>
    [Fact]
    public async Task Create_ShouldRefuseAVenueAlreadyTaken()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldRefuseAVenueAlreadyTaken));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());

        await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);

        var act = () => handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9).AddMinutes(30)),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Message.ShouldContain("déjà occupé");
    }

    /// <summary>
    /// Touching at the boundary is not overlapping: a class ending at 10:00 and
    /// the next starting at 10:00 share the room for no time at all.
    /// </summary>
    [Fact]
    public async Task Create_ShouldAllowBackToBackSessions()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldAllowBackToBackSessions));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);
        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());

        await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);
        await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(10)), CancellationToken.None);

        dbContext.Sessions.Count().ShouldBe(2);
    }

    /// <summary>Invariant 3: a coach is never on two sessions at once.</summary>
    [Fact]
    public async Task Create_ShouldRefuseACoachAlreadyRunningASession()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldRefuseACoachAlreadyRunningASession));
        var (template, location, coach) = await SeedCatalogueAsync(dbContext);
        var otherStudio = new Location("Studio B") { Capacity = 20 };
        dbContext.Locations.Add(otherStudio);
        await dbContext.SaveChangesAsync();

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9), coach.Id), CancellationToken.None);

        var act = () => handler.Handle(
            new CreateSessionCommand(template.Id, otherStudio.Id, NextMonday(9).AddMinutes(15), coach.Id),
            CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Message.ShouldContain("Nora Lemoine");
    }

    /// <summary>
    /// A coach on leave is an alert the drawer raises, not a refusal: blocking
    /// the write would stop a manager covering the gap.
    /// </summary>
    [Fact]
    public async Task Create_ShouldAcceptACoachOnLeave()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Create_ShouldAcceptACoachOnLeave));
        var (template, location, coach) = await SeedCatalogueAsync(dbContext);
        coach.AwayUntil = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        await dbContext.SaveChangesAsync();

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        var id = await handler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9), coach.Id), CancellationToken.None);

        id.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Moving a series keeps every occurrence's own place in the calendar,
    /// instead of collapsing the term onto one date.
    /// </summary>
    [Fact]
    public async Task Update_ShouldShiftTheFollowingOccurrences()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Update_ShouldShiftTheFollowingOccurrences));
        var (template, location, coach) = await SeedCatalogueAsync(dbContext);
        var start = NextMonday(9);

        var createHandler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        await createHandler.Handle(
            new CreateSessionCommand(template.Id, location.Id, start, coach.Id, recurrenceWeeks: 3),
            CancellationToken.None);

        var second = dbContext.Sessions.OrderBy(session => session.StartsAt).Skip(1).First();

        var updateHandler = new UpdateSessionCommandHandler(dbContext, new UpdateSessionCommandValidator());
        var updated = await updateHandler.Handle(
            new UpdateSessionCommand(
                second.Id,
                location.Id,
                second.StartsAt.AddHours(1),
                coach.Id,
                scope: SessionEditScope.ThisAndFollowing),
            CancellationToken.None);

        updated.ShouldBeTrue();

        var hours = dbContext.Sessions
            .OrderBy(session => session.StartsAt)
            .Select(session => session.StartsAt.Hour)
            .ToList();
        hours.ShouldBe([9, 10, 10]);
    }

    /// <summary>
    /// Shrinking a session below the seats already taken would throw members out
    /// of a class they are booked on.
    /// </summary>
    [Fact]
    public async Task Update_ShouldRefuseACapacityBelowTheSeatsTaken()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Update_ShouldRefuseACapacityBelowTheSeatsTaken));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);

        var createHandler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        var id = await createHandler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);

        dbContext.Registrations.AddRange(
            Enumerable.Range(0, 5).Select(seat => new Registration
            {
                SessionId = id,
                Member = new Member($"Member{seat}", "Test")
            }));
        await dbContext.SaveChangesAsync();

        var updateHandler = new UpdateSessionCommandHandler(dbContext, new UpdateSessionCommandValidator());

        var act = () => updateHandler.Handle(
            new UpdateSessionCommand(id, location.Id, NextMonday(9), capacity: 3), CancellationToken.None);

        var error = await act.ShouldThrowAsync<ValidationException>();
        error.Message.ShouldContain("5 inscrits");
    }

    /// <summary>
    /// A cancelled session keeps its row: the members who had a seat have to see
    /// why, and the history has to keep the slot. It simply stops holding the
    /// venue.
    /// </summary>
    [Fact]
    public async Task Cancel_ShouldKeepTheRowAndFreeTheSlot()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Cancel_ShouldKeepTheRowAndFreeTheSlot));
        var (template, location, _) = await SeedCatalogueAsync(dbContext);

        var createHandler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        var id = await createHandler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);

        var cancelHandler = new CancelSessionCommandHandler(dbContext, new TestEmailSender(), new TestTenantContext(TestInfrastructure.DefaultTenantId), new CancelSessionCommandValidator());
        var cancelled = await cancelHandler.Handle(
            new CancelSessionCommand(id, "Coach malade"), CancellationToken.None);

        cancelled.IsSaved.ShouldBeTrue();
        var session = dbContext.Sessions.Single(candidate => candidate.Id == id);
        session.Status.ShouldBe(SessionStatus.Cancelled);
        session.CancellationReason.ShouldBe("Coach malade");
        session.IsActive.ShouldBeTrue();

        // The venue is free again, so the slot can be rebooked.
        var rebooked = await createHandler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9)), CancellationToken.None);
        rebooked.ShouldBeGreaterThan(0);
    }

    /// <summary>Cancelling the rest of a term never reaches back into what already ran.</summary>
    [Fact]
    public async Task Cancel_ShouldNotReachBackIntoThePast()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Cancel_ShouldNotReachBackIntoThePast));
        var (template, location, coach) = await SeedCatalogueAsync(dbContext);

        var createHandler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator());
        await createHandler.Handle(
            new CreateSessionCommand(template.Id, location.Id, NextMonday(9), coach.Id, recurrenceWeeks: 4),
            CancellationToken.None);

        var third = dbContext.Sessions.OrderBy(session => session.StartsAt).Skip(2).First();

        var cancelHandler = new CancelSessionCommandHandler(dbContext, new TestEmailSender(), new TestTenantContext(TestInfrastructure.DefaultTenantId), new CancelSessionCommandValidator());
        await cancelHandler.Handle(
            new CancelSessionCommand(third.Id, "Fermeture", SessionEditScope.ThisAndFollowing),
            CancellationToken.None);

        var statuses = dbContext.Sessions
            .OrderBy(session => session.StartsAt)
            .Select(session => session.Status)
            .ToList();

        statuses.ShouldBe([
            SessionStatus.Scheduled,
            SessionStatus.Scheduled,
            SessionStatus.Cancelled,
            SessionStatus.Cancelled
        ]);
    }

    [Fact]
    public async Task Cancel_ShouldReturnFalse_WhenSessionNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Cancel_ShouldReturnFalse_WhenSessionNotFound));
        var handler = new CancelSessionCommandHandler(dbContext, new TestEmailSender(), new TestTenantContext(TestInfrastructure.DefaultTenantId), new CancelSessionCommandValidator());

        var cancelled = await handler.Handle(new CancelSessionCommand(4321), CancellationToken.None);

        cancelled.IsSaved.ShouldBeFalse();
    }

    private static async Task<(CourseTemplate Template, Location Location, Coach Coach)> SeedCatalogueAsync(
        GymDbContext dbContext)
    {
        var template = new CourseTemplate("HIIT Blast")
        {
            Discipline = new Discipline("HIIT"),
            Capacity = 16,
            DurationMinutes = 60
        };
        var location = new Location("Studio A") { Capacity = 20 };
        var coach = new Coach("Nora", "Lemoine");

        dbContext.CourseTemplates.Add(template);
        dbContext.Locations.Add(location);
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        return (template, location, coach);
    }

    /// <summary>A fixed anchor in the future, so no test depends on the day it runs.</summary>
    private static DateTime NextMonday(int hour) =>
        PlanningRules.MondayOf(DateTime.Today).AddDays(7).AddHours(hour);
}
