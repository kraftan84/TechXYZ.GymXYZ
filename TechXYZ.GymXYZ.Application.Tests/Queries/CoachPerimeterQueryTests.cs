using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// What a salaried coach reads and writes, end to end through the handlers.
/// <para>
/// Every test seeds the same gym — two coaches, a class each — and asks the
/// question twice: once as the coach, once as the manager. The second half is
/// the one that matters most, because the failure this lot could cause is not
/// "a coach saw too much" but "the manager quietly lost their gym".
/// </para>
/// </summary>
public class CoachPerimeterQueryTests
{
    [Fact]
    public async Task Attendance_ShouldListOnlyTheCoachsOwnSheets()
    {
        await using var dbContext = Gym(nameof(Attendance_ShouldListOnlyTheCoachsOwnSheets));
        var (nora, samir) = await SeedTwoCoachesAsync(dbContext);

        var asCoach = await Overview(dbContext, TestCurrentUserService.Coach(nora.Id));
        var asManager = await Overview(dbContext, TestCurrentUserService.Manager());

        asCoach.ToPoint.Select(session => session.CoachFirstName).ShouldAllBe(name => name == "Nora");
        asCoach.ToPoint.Count.ShouldBe(1);
        asManager.ToPoint.Count.ShouldBe(2, "The manager still sees the whole gym.");
        samir.Id.ShouldNotBe(nora.Id);
    }

    [Fact]
    public async Task Attendance_ShouldCountTheSameSheetsItLists()
    {
        // The badge, the KPI and the list all read one place. A count that
        // disagreed with the rows underneath would read as a lost session.
        await using var dbContext = Gym(nameof(Attendance_ShouldCountTheSameSheetsItLists));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);

        var overview = await Overview(dbContext, TestCurrentUserService.Coach(nora.Id));

        overview.Kpis.SheetsToPoint.ShouldBe(overview.ToPoint.Count);
    }

    [Fact]
    public async Task Dashboard_ShouldCarryNoAmount_ForACoach()
    {
        await using var dbContext = Gym(nameof(Dashboard_ShouldCarryNoAmount_ForACoach));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);

        var asCoach = await new GetDashboardQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(new GetDashboardQuery(), CancellationToken.None);

        asCoach.Alerts.LateAmount.ShouldBe(0m, "A coach is not shown what the club is owed.");
        asCoach.Alerts.LateCount.ShouldBe(0);
        asCoach.Alerts.ExpiringCount.ShouldBe(0);
        asCoach.Alerts.SheetsToPoint.ShouldBe(1, "Their own sheets stay, or the page has nothing to say.");
    }

    [Fact]
    public async Task Dashboard_ShouldShowOnlyTheCoachsOwnWeek()
    {
        await using var dbContext = Gym(nameof(Dashboard_ShouldShowOnlyTheCoachsOwnWeek));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);

        var asCoach = await new GetDashboardQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(new GetDashboardQuery(), CancellationToken.None);
        var asManager = await new GetDashboardQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetDashboardQuery(), CancellationToken.None);

        asCoach.WeekCoachCount.ShouldBe(1);
        asManager.WeekCoachCount.ShouldBe(2);
    }

    [Fact]
    public async Task Planning_ShouldIgnoreACoachsAttemptToWidenTheFilter()
    {
        // Request.CoachId is a toolbar chip the caller picks. It may narrow the
        // week further; asking for a colleague's must not widen it.
        await using var dbContext = Gym(nameof(Planning_ShouldIgnoreACoachsAttemptToWidenTheFilter));
        var (nora, samir) = await SeedTwoCoachesAsync(dbContext);

        var week = await new GetWeekPlanningQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(
                new GetWeekPlanningQuery(DateOnly.FromDateTime(DateTime.Today)) { CoachId = samir.Id },
                CancellationToken.None);

        week.Sessions.ShouldBeEmpty(
            "Asking for somebody else's week answers with nothing, not with theirs.");
    }

    [Fact]
    public async Task Members_ShouldListOnlyThePeopleTheCoachTeaches()
    {
        await using var dbContext = Gym(nameof(Members_ShouldListOnlyThePeopleTheCoachTeaches));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);

        var asCoach = await new GetMembersQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(new GetMembersQuery(), CancellationToken.None);
        var asManager = await new GetMembersQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetMembersQuery(), CancellationToken.None);

        asCoach.Items.Select(member => member.FirstName).ShouldBe(["Alice"]);
        asManager.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task MemberDetails_ShouldNotOpenSomebodyTheCoachNeverTeaches()
    {
        // The URL that walks around the list. Answered as "not found" rather
        // than as a refusal, because the coach cannot see this person anywhere.
        await using var dbContext = Gym(nameof(MemberDetails_ShouldNotOpenSomebodyTheCoachNeverTeaches));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);
        var bob = dbContext.Members.Single(member => member.FirstName == "Bob");

        var fiche = await new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(new GetMemberDetailsPageQuery(bob.Id), CancellationToken.None);

        fiche.ShouldBeNull();
    }

    [Fact]
    public async Task MemberDetails_ShouldCarryNoMoneyOrPrivateDetail_ForACoach()
    {
        await using var dbContext = Gym(nameof(MemberDetails_ShouldCarryNoMoneyOrPrivateDetail_ForACoach));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);
        var alice = dbContext.Members.Single(member => member.FirstName == "Alice");

        var asCoach = await new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Coach(nora.Id))
            .Handle(new GetMemberDetailsPageQuery(alice.Id), CancellationToken.None);
        var asManager = await new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetMemberDetailsPageQuery(alice.Id), CancellationToken.None);

        asCoach.ShouldNotBeNull();
        asCoach.Payments.ShouldBeEmpty();
        asCoach.Subscriptions.ShouldBeEmpty();
        asCoach.Address.ShouldBeNull();
        asCoach.BirthDate.ShouldBeNull();
        asCoach.Notes.ShouldBeNull();

        // Kept, because a coach has to reach the person and know they are covered.
        asCoach.Email.ShouldBe("alice@gymxyz.fr");
        asCoach.Phone.ShouldNotBeNull();

        asManager.ShouldNotBeNull();
        asManager.Notes.ShouldNotBeNull("The manager's fiche is untouched.");
        asManager.Address.ShouldNotBeNull();
    }

    [Fact]
    public async Task Attendance_ShouldRefuseToPointSomebodyElsesSheet()
    {
        await using var dbContext = Gym(nameof(Attendance_ShouldRefuseToPointSomebodyElsesSheet));
        var (nora, samir) = await SeedTwoCoachesAsync(dbContext);
        var samirsSession = dbContext.Sessions.Single(session => session.CoachId == samir.Id);

        var handler = new MarkWholeSheetCommandHandler(
            dbContext, new MarkWholeSheetCommandValidator(), TestCurrentUserService.Coach(nora.Id));

        var error = await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(
                new MarkWholeSheetCommand(samirsSession.Id, AttendanceStatus.Present),
                CancellationToken.None));

        error.Errors.First().ErrorMessage.ShouldBe(AttendanceRules.SessionNotFound);
    }

    [Fact]
    public async Task Attendance_ShouldLetACoachPointTheirOwnSheet()
    {
        // The direction that matters: the perimeter must not take away the work
        // the person came to do.
        await using var dbContext = Gym(nameof(Attendance_ShouldLetACoachPointTheirOwnSheet));
        var (nora, _) = await SeedTwoCoachesAsync(dbContext);
        var own = dbContext.Sessions.Single(session => session.CoachId == nora.Id);

        var handler = new MarkWholeSheetCommandHandler(
            dbContext, new MarkWholeSheetCommandValidator(), TestCurrentUserService.Coach(nora.Id));

        var marked = await handler.Handle(
            new MarkWholeSheetCommand(own.Id, AttendanceStatus.Present), CancellationToken.None);

        marked.ShouldBeTrue();
    }

    private static GymDbContext Gym(string name) => TestInfrastructure.CreateDbContext(name);

    /// <summary>
    /// Two coaches, one past class each, one member booked onto each. Alice is
    /// Nora's; Bob is Samir's and is the person a coach must not be able to open.
    /// </summary>
    private static async Task<(Coach Nora, Coach Samir)> SeedTwoCoachesAsync(GymDbContext dbContext)
    {
        var nora = new Coach("Nora", "Lemoine");
        var samir = new Coach("Samir", "El Amrani");
        dbContext.Coaches.AddRange(nora, samir);

        var alice = new Member("Alice", "Girard")
        {
            Email = "alice@gymxyz.fr",
            Phone = "06 11 22 33 44",
            BirthDate = new DateOnly(1990, 4, 2),
            Notes = "Épaule droite fragile.",
            Address = new Address { Street = "1 rue A", ZipCode = "69003", City = "Lyon", Country = "France" }
        };

        var bob = new Member("Bob", "Martin")
        {
            Email = "bob@gymxyz.fr",
            Notes = "Rien à signaler."
        };

        dbContext.Members.AddRange(alice, bob);

        var yesterday = DateTime.Today.AddDays(-1).AddHours(18);

        dbContext.Sessions.AddRange(
            NewSession(nora, alice, yesterday, "HIIT Blast"),
            NewSession(samir, bob, yesterday.AddHours(1), "Power Cycle"));

        await dbContext.SaveChangesAsync();

        return (nora, samir);
    }

    private static Session NewSession(Coach coach, Member member, DateTime startsAt, string courseName) =>
        new()
        {
            CourseTemplate = new CourseTemplate(courseName)
            {
                Discipline = new Discipline(courseName),
                DurationMinutes = 60,
                Capacity = 12
            },
            Location = new Location("Studio A") { Kind = LocationKind.Studio, Capacity = 24 },
            Coach = coach,
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            Capacity = 12,
            Registrations = [new Registration { Member = member }]
        };

    private static Task<AttendanceOverviewDto> Overview(
        GymDbContext dbContext, TestCurrentUserService user) =>
        new GetAttendanceOverviewQueryHandler(dbContext, user)
            .Handle(new GetAttendanceOverviewQuery(), CancellationToken.None);
}
