using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// What goes on the image the manager publishes — and, more to the point, what
/// never does.
/// </summary>
public class PlanningPosterQueryHandlerTests
{
    /// <summary>
    /// The poster is reserved to whoever runs the gym. Asserted as a property of
    /// the query rather than by calling the handler, because the refusal is the
    /// pipeline's job — <c>ManagerOnlyBehaviourTests</c> owns that half, and a
    /// handler called directly in a test would never see it.
    /// </summary>
    [Fact]
    public void TheQuery_ShouldBeReservedToAManager()
    {
        typeof(GetPlanningPosterQuery).IsAssignableTo(typeof(IManagerOnly)).ShouldBeTrue(
            "a coach generating a poster would publish their own week as the club's.");
    }

    /// <summary>
    /// A private session is somebody's appointment. It is off the poster whatever
    /// the mockup draws, and the rule is not the coach's to remember.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLeavePrivateSessionsOff()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldLeavePrivateSessionsOff));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "HIIT Blast", monday, hour: 9, capacity: 16);
        Seed(dbContext, "Coaching perso", monday, hour: 11, capacity: 1);
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday);

        result.Sessions.Select(session => session.CourseName).ShouldBe(["HIIT Blast"]);
    }

    /// <summary>A cancelled class must not be advertised: nobody can come to it.</summary>
    [Fact]
    public async Task Handle_ShouldLeaveCancelledAndArchivedSessionsOff()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldLeaveCancelledAndArchivedSessionsOff));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "Pilates Core", monday, hour: 9);
        Seed(dbContext, "Cancelled", monday, hour: 10).Status = SessionStatus.Cancelled;
        Seed(dbContext, "Archived", monday, hour: 11).IsActive = false;
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday);

        result.Sessions.Select(session => session.CourseName).ShouldBe(["Pilates Core"]);
    }

    /// <summary>
    /// « Studio B · 4 places » is what is <b>left</b>. The waiting list does not
    /// take a seat away, and a class with more people queueing than it seats
    /// never advertises a negative number.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAdvertiseRemainingSeatsOnly()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldAdvertiseRemainingSeatsOnly));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        var roomy = Seed(dbContext, "Yoga Restore", monday, hour: 9, capacity: 6);
        roomy.Registrations =
        [
            new Registration { Member = new Member("A", "One") },
            new Registration { Member = new Member("B", "Two") },
            new Registration { Member = new Member("C", "Three"), IsWaitlisted = true },
            new Registration { Member = new Member("D", "Four"), IsActive = false }
        ];

        var full = Seed(dbContext, "Power Cycle", monday, hour: 18, capacity: 2);
        full.Registrations =
        [
            new Registration { Member = new Member("E", "Five") },
            new Registration { Member = new Member("F", "Six") },
            new Registration { Member = new Member("G", "Seven"), IsWaitlisted = true }
        ];
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday);

        var yoga = result.Sessions.Single(session => session.CourseName == "Yoga Restore");
        yoga.RemainingSeats.ShouldBe(4);
        yoga.IsFull.ShouldBeFalse();

        var cycle = result.Sessions.Single(session => session.CourseName == "Power Cycle");
        cycle.RemainingSeats.ShouldBe(0);
        cycle.IsFull.ShouldBeTrue();
    }

    /// <summary>
    /// The header counts the classes and the layout reads the busiest day, which
    /// is what decides how many columns every day is drawn with.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReportTheBusiestDayOfTheWeek()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReportTheBusiestDayOfTheWeek));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "One", monday, hour: 9);
        Seed(dbContext, "Two", monday.AddDays(2), hour: 9);
        Seed(dbContext, "Three", monday.AddDays(2), hour: 12);
        Seed(dbContext, "Four", monday.AddDays(2), hour: 18);
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday);

        result.CourseCount.ShouldBe(4);
        result.BusiestDay.ShouldBe(3);
        result.On(monday.AddDays(2)).Count.ShouldBe(3);
        result.On(monday.AddDays(1)).ShouldBeEmpty();
    }

    /// <summary>
    /// The button hands over the week its screen is showing, whichever day that
    /// is — the Accueil knows a Saturday, the Planning knows whatever the arrows
    /// left it on.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldNormalizeTheAnchorToItsMonday()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldNormalizeTheAnchorToItsMonday));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        Seed(dbContext, "Sunday", monday.AddDays(6), hour: 10);
        Seed(dbContext, "Next Monday", monday.AddDays(7), hour: 9);
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday.AddDays(4));

        result.WeekStart.ShouldBe(monday);
        result.WeekEnd.ShouldBe(monday.AddDays(6));
        result.Sessions.Select(session => session.CourseName).ShouldBe(["Sunday"]);
    }

    /// <summary>
    /// Duration and the abbreviated coach name are what Team Trainer's writes
    /// under a class — « 60 min · L. Fontaine ». An unstaffed slot has no name to
    /// abbreviate and says so with a null rather than a stray full stop.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCarryDurationAndTheShortCoachName()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCarryDurationAndTheShortCoachName));
        var monday = PlanningRules.MondayOf(DateOnly.FromDateTime(DateTime.Today));

        var coached = Seed(dbContext, "Functional", monday, hour: 17);
        coached.Coach = new Coach("Lily", "Fontaine");
        coached.EndsAt = coached.StartsAt.AddMinutes(45);

        Seed(dbContext, "Open Gym", monday, hour: 20);
        await dbContext.SaveChangesAsync();

        var result = await Poster(dbContext, monday);

        var functional = result.Sessions.Single(session => session.CourseName == "Functional");
        functional.CoachShortName.ShouldBe("L. Fontaine");
        functional.DurationMinutes.ShouldBe(45);

        result.Sessions.Single(session => session.CourseName == "Open Gym").CoachShortName.ShouldBeNull();
    }

    private static async Task<TechXyz.GymXyz.Application.Models.PosterWeekDto> Poster(
        GymDbContext dbContext,
        DateOnly anchor) =>
        await new GetPlanningPosterQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetPlanningPosterQuery(anchor), CancellationToken.None);

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
