using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class WeekPlanningQueryHandlerTests
{
    /// <summary>
    /// The grid draws the week the caller is looking at and nothing else, so a
    /// session on the Sunday belongs to it and the next Monday does not.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnTheWeekOnly()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnTheWeekOnly));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "Sunday", monday.AddDays(6), hour: 10);
        Seed(dbContext, "Monday", monday, hour: 9);
        Seed(dbContext, "Next Monday", monday.AddDays(7), hour: 9);
        Seed(dbContext, "Last Sunday", monday.AddDays(-1), hour: 9);
        await dbContext.SaveChangesAsync();

        var handler = new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetWeekPlanningQuery(monday), CancellationToken.None);

        result.WeekStart.ShouldBe(monday);
        result.WeekEnd.ShouldBe(monday.AddDays(6));
        result.Sessions.Select(session => session.CourseName).ShouldBe(["Monday", "Sunday"]);
    }

    /// <summary>
    /// Any day of the week is a valid anchor: the toolbar hands over the date it
    /// is showing, not a Monday it had to work out first.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldNormalizeTheAnchorToItsMonday()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldNormalizeTheAnchorToItsMonday));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "Monday", monday, hour: 9);
        await dbContext.SaveChangesAsync();

        var handler = new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetWeekPlanningQuery(monday.AddDays(4)), CancellationToken.None);

        result.WeekStart.ShouldBe(monday);
        result.Sessions.Count.ShouldBe(1);
    }

    /// <summary>
    /// Occupancy is what the block writes as "14/20", and a seat on the waiting
    /// list is not an occupied one — counting it would show a full class before
    /// it is.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCountSeatsWithoutTheWaitingList()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCountSeatsWithoutTheWaitingList));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        var session = Seed(dbContext, "Power Cycle", monday, hour: 18, capacity: 4);
        session.Registrations =
        [
            new Registration { Member = new Member("A", "One") },
            new Registration { Member = new Member("B", "Two") },
            new Registration { Member = new Member("C", "Three"), IsWaitlisted = true },
            new Registration { Member = new Member("D", "Four"), IsActive = false }
        ];
        await dbContext.SaveChangesAsync();

        var handler = new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetWeekPlanningQuery(monday), CancellationToken.None);

        result.Sessions[0].Registered.ShouldBe(2);
        result.Sessions[0].Capacity.ShouldBe(4);
        result.Sessions[0].IsFull.ShouldBeFalse();
    }

    /// <summary>The three toolbar chips narrow the grid server-side.</summary>
    [Fact]
    public async Task Handle_ShouldApplyTheToolbarFilters()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldApplyTheToolbarFilters));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        var nora = new Coach("Nora", "Lemoine");
        var studioA = new Location("Studio A") { Capacity = 20 };

        var collective = Seed(dbContext, "HIIT Blast", monday, hour: 9, capacity: 16);
        collective.Coach = nora;
        collective.Location = studioA;

        var priv = Seed(dbContext, "Coaching Perso", monday, hour: 11, capacity: 1);

        await dbContext.SaveChangesAsync();

        var handler = new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Manager());

        var privateOnly = await handler.Handle(
            new GetWeekPlanningQuery(monday) { Format = CourseFormat.Private }, CancellationToken.None);
        privateOnly.Sessions.Select(session => session.CourseName).ShouldBe(["Coaching Perso"]);
        privateOnly.Sessions[0].IsPrivate.ShouldBeTrue();

        var byCoach = await handler.Handle(
            new GetWeekPlanningQuery(monday) { CoachId = nora.Id }, CancellationToken.None);
        byCoach.Sessions.Select(session => session.CourseName).ShouldBe(["HIIT Blast"]);
        byCoach.Sessions[0].CoachShortName.ShouldBe("N. Lemoine");

        var byLocation = await handler.Handle(
            new GetWeekPlanningQuery(monday) { LocationId = priv.LocationId }, CancellationToken.None);
        byLocation.Sessions.Select(session => session.CourseName).ShouldBe(["Coaching Perso"]);
    }

    /// <summary>
    /// A slot nobody animates still belongs on the planning — the open-access
    /// plateau is the case the prototype draws with a dash.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCarryASessionWithoutACoach()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCarryASessionWithoutACoach));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "Open Gym", monday, hour: 10, capacity: 30);
        await dbContext.SaveChangesAsync();

        var handler = new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetWeekPlanningQuery(monday), CancellationToken.None);

        result.Sessions[0].CoachShortName.ShouldBeNull();
        result.Sessions[0].DayIndex.ShouldBe(0);
        result.Sessions[0].Hour.ShouldBe(10);
    }

    private static Session Seed(
        GymDbContext dbContext,
        string courseName,
        DateOnly day,
        int hour,
        int capacity = 16)
    {
        var startsAt = day.ToDateTime(new TimeOnly(hour, 0));

        var session = new Session
        {
            CourseTemplate = new CourseTemplate(courseName)
            {
                Discipline = new Discipline($"{courseName} discipline"),
                Capacity = capacity,
                DurationMinutes = 60
            },
            Location = new Location($"{courseName} venue") { Capacity = capacity },
            Capacity = capacity,
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1)
        };

        dbContext.Sessions.Add(session);

        return session;
    }
}
