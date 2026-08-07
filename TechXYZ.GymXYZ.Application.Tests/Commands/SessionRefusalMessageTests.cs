using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// A refused write has to say why, and the toast is built from the exception's
/// <c>Errors</c> collection — not from its message. Throwing
/// <c>new ValidationException(text)</c> leaves that collection empty and the
/// user reads "Validation invalide" instead of which room is taken, so every
/// invariant is pinned here to carry a failure.
/// </summary>
public class SessionRefusalMessageTests
{
    [Fact]
    public async Task OverCapacity_ShouldNameTheVenueAndItsLimit()
    {
        var error = await RefusalOf(async (handler, seed) =>
            await handler.Handle(
                new CreateSessionCommand(seed.Template.Id, seed.Location.Id, NextMonday(9), capacity: 40),
                CancellationToken.None));

        error.Errors.ShouldNotBeEmpty();
        error.Errors.First().ErrorMessage.ShouldBe("Studio A ne peut accueillir que 20 personnes.");
    }

    [Fact]
    public async Task BusyVenue_ShouldNameTheVenueAndTheMoment()
    {
        var error = await RefusalOf(async (handler, seed) =>
        {
            await handler.Handle(
                new CreateSessionCommand(seed.Template.Id, seed.Location.Id, NextMonday(9)), CancellationToken.None);
            await handler.Handle(
                new CreateSessionCommand(seed.Template.Id, seed.Location.Id, NextMonday(9).AddMinutes(30)),
                CancellationToken.None);
        });

        error.Errors.ShouldNotBeEmpty();
        error.Errors.First().ErrorMessage.ShouldStartWith("Studio A est déjà occupé le lundi");
    }

    /// <summary>
    /// In the other studio, so the venue is free and it really is the coach who
    /// is double-booked — the venue is checked first.
    /// </summary>
    [Fact]
    public async Task BusyCoach_ShouldNameTheCoachAndTheMoment()
    {
        var error = await RefusalOf(async (handler, seed) =>
        {
            await handler.Handle(
                new CreateSessionCommand(seed.Template.Id, seed.Location.Id, NextMonday(9), seed.Coach.Id),
                CancellationToken.None);
            await handler.Handle(
                new CreateSessionCommand(
                    seed.Template.Id, seed.OtherLocation.Id, NextMonday(9).AddMinutes(15), seed.Coach.Id),
                CancellationToken.None);
        });

        error.Errors.ShouldNotBeEmpty();
        error.Errors.First().ErrorMessage.ShouldStartWith("Nora Lemoine anime déjà une séance le lundi");
    }

    private sealed record Seed(CourseTemplate Template, Location Location, Location OtherLocation, Coach Coach);

    private static async Task<ValidationException> RefusalOf(
        Func<CreateSessionCommandHandler, Seed, Task> act,
        [System.Runtime.CompilerServices.CallerMemberName] string databaseName = "")
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName);

        var template = new CourseTemplate("HIIT Blast")
        {
            Discipline = new Discipline("HIIT"),
            Capacity = 16,
            DurationMinutes = 60
        };
        var location = new Location("Studio A") { Capacity = 20 };
        var otherLocation = new Location("Studio B") { Capacity = 20 };
        var coach = new Coach("Nora", "Lemoine");

        dbContext.CourseTemplates.Add(template);
        dbContext.Locations.AddRange(location, otherLocation);
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new CreateSessionCommandHandler(dbContext, new CreateSessionCommandValidator(), TestCurrentUserService.Manager());

        return await Should.ThrowAsync<ValidationException>(
            async () => await act(handler, new Seed(template, location, otherLocation, coach)));
    }

    private static DateTime NextMonday(int hour) =>
        PlanningRules.MondayOf(DateTime.Today).AddDays(7).AddHours(hour);
}
