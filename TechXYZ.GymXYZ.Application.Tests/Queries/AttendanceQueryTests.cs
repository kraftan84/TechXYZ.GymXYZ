using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The two reads behind the Présences screen: the overview, and one sheet.
/// </summary>
public class AttendanceQueryTests
{
    [Fact]
    public async Task GetSessionRoster_ShouldTallyTheSheetAndRateIt()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetSessionRoster_ShouldTallyTheSheetAndRateIt));
        var session = SeedSession(dbContext, Yesterday(18), present: 6, late: 1, absent: 1, pending: 2);
        await dbContext.SaveChangesAsync();

        var result = await Roster(dbContext).Handle(new GetSessionRosterQuery(session.Id), CancellationToken.None);

        result.Present.ShouldBe(6);
        result.Late.ShouldBe(1);
        result.Absent.ShouldBe(1);
        result.Pending.ShouldBe(2);

        // 7 of the 8 seats anybody pointed. The two nobody reached are not
        // absences and must not count against the class.
        result.AttendanceRate.ShouldBe(88);
    }

    /// <summary>
    /// A sheet nobody opened has no rate. Nought per cent would be a verdict on a
    /// class that may well have been full, and the band shows "—" for it.
    /// </summary>
    [Fact]
    public async Task GetSessionRoster_ShouldHaveNoRate_WhenNothingWasPointed()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetSessionRoster_ShouldHaveNoRate_WhenNothingWasPointed));
        var session = SeedSession(dbContext, Today(9), pending: 12);
        await dbContext.SaveChangesAsync();

        var result = await Roster(dbContext).Handle(new GetSessionRosterQuery(session.Id), CancellationToken.None);

        result.Marked.ShouldBe(0);
        result.AttendanceRate.ShouldBeNull();
        result.IsClosed.ShouldBeFalse();
    }

    /// <summary>
    /// The roster projects a collection inside a projection, ordered by a
    /// navigation. That passes on the in-memory provider and is exactly the shape
    /// that fails to translate on a relational one, so it is pinned against
    /// SQLite as well.
    /// </summary>
    [Fact]
    public async Task GetSessionRoster_ShouldTranslateOnARelationalProvider()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var session = SeedSession(scope.DbContext, Yesterday(18), present: 3, absent: 1);
        await scope.DbContext.SaveChangesAsync();

        var result = await Roster(scope.DbContext).Handle(new GetSessionRosterQuery(session.Id), CancellationToken.None);

        result.Seats.Count.ShouldBe(4);
        result.Present.ShouldBe(3);
        result.AttendanceRate.ShouldBe(75);
    }

    [Fact]
    public async Task GetSessionRoster_ShouldKeepTheWaitingListOutOfTheTally()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetSessionRoster_ShouldKeepTheWaitingListOutOfTheTally));
        var session = SeedSession(dbContext, Yesterday(18), present: 4, waitlisted: 2);
        await dbContext.SaveChangesAsync();

        var result = await Roster(dbContext).Handle(new GetSessionRosterQuery(session.Id), CancellationToken.None);

        result.Seats.Count.ShouldBe(6);
        result.Registered.Count.ShouldBe(4);
        result.AttendanceRate.ShouldBe(100);
    }

    [Fact]
    public async Task GetSessionRoster_ShouldNotOfferTheReopenToACoach()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetSessionRoster_ShouldNotOfferTheReopenToACoach));
        var session = SeedSession(dbContext, Yesterday(18), present: 4);
        session.AttendanceClosedAt = Yesterday(19);
        await dbContext.SaveChangesAsync();

        var result = await Roster(dbContext, GymRoleNames.Coach)
            .Handle(new GetSessionRosterQuery(session.Id), CancellationToken.None);

        result.IsClosed.ShouldBeTrue();
        result.CanReopen.ShouldBeFalse();
    }

    [Fact]
    public async Task GetSessionRoster_ShouldOfferTheReopenToAManager_OnlyOnceValidated()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetSessionRoster_ShouldOfferTheReopenToAManager_OnlyOnceValidated));
        var open = SeedSession(dbContext, Today(9), pending: 4);
        var closed = SeedSession(dbContext, Yesterday(18), present: 4);
        closed.AttendanceClosedAt = Yesterday(19);
        await dbContext.SaveChangesAsync();

        var handler = Roster(dbContext, GymRoleNames.GymManager);

        (await handler.Handle(new GetSessionRosterQuery(open.Id), CancellationToken.None))
            .CanReopen.ShouldBeFalse();
        (await handler.Handle(new GetSessionRosterQuery(closed.Id), CancellationToken.None))
            .CanReopen.ShouldBeTrue();
    }

    /// <summary>
    /// The two lists of the screen: sheets still open on one side, validated ones
    /// on the other.
    /// </summary>
    [Fact]
    public async Task GetAttendanceOverview_ShouldSplitOpenAndValidatedSheets()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldSplitOpenAndValidatedSheets));
        var toPoint = SeedSession(dbContext, Today(9), pending: 12, courseName: "HIIT Blast");
        var pointed = SeedSession(dbContext, Yesterday(18), present: 8, absent: 2, courseName: "Pilates Core");
        pointed.AttendanceClosedAt = Yesterday(19);
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.ToPoint.Select(session => session.Id).ShouldBe([toPoint.Id]);
        result.Pointed.Select(session => session.Id).ShouldBe([pointed.Id]);
        result.Kpis.SheetsToPoint.ShouldBe(1);
        result.Kpis.SessionsToday.ShouldBe(1);
    }

    /// <summary>
    /// A cancelled class never had a sheet, so it is in neither list and counts
    /// towards nothing.
    /// </summary>
    [Fact]
    public async Task GetAttendanceOverview_ShouldIgnoreCancelledSessions()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldIgnoreCancelledSessions));
        var cancelled = SeedSession(dbContext, Today(9), pending: 12);
        cancelled.Status = SessionStatus.Cancelled;
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.ToPoint.ShouldBeEmpty();
        result.Kpis.SheetsToPoint.ShouldBe(0);
        result.Kpis.AttendanceRate.ShouldBeNull();
    }

    [Fact]
    public async Task GetAttendanceOverview_ShouldRankCoursesByAttendance()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldRankCoursesByAttendance));
        var cycle = NewTemplate("Power Cycle");
        var boxing = NewTemplate("Boxing Fundamentals");

        SeedSession(dbContext, Yesterday(18), present: 19, absent: 1, template: cycle);
        SeedSession(dbContext, Yesterday(19), present: 7, absent: 3, template: boxing);
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.CourseRates.Select(course => course.CourseName).ShouldBe(["Power Cycle", "Boxing Fundamentals"]);
        result.CourseRates[0].Rate.ShouldBe(95);
        result.CourseRates[1].Rate.ShouldBe(70);
    }

    /// <summary>
    /// A one-to-one is not compared against a class. Its no-shows still count
    /// towards the KPIs; it is only the ranking they would distort, since a
    /// session seating one is full the moment it is booked.
    /// </summary>
    [Fact]
    public async Task GetAttendanceOverview_ShouldKeepPrivateSessionsOutOfTheBars()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldKeepPrivateSessionsOutOfTheBars));

        var solo = SeedSession(dbContext, Yesterday(11), template: NewTemplate("Coaching Perso"));
        solo.Capacity = 1;
        solo.Registrations =
        [
            new Registration { Member = new Member("Solo", "Test"), Status = AttendanceStatus.Absent }
        ];

        SeedSession(dbContext, Yesterday(18), present: 10, template: NewTemplate("Power Cycle"));
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.CourseRates.Select(course => course.CourseName).ShouldBe(["Power Cycle"]);

        // The missed one-to-one is still a no-show of the week.
        result.Kpis.NoShowsThisWeek.ShouldBe(1);
    }

    /// <summary>
    /// A course whose sheets nobody pointed drops out of the bars rather than
    /// showing at nought — the same rule the rate itself follows.
    /// </summary>
    [Fact]
    public async Task GetAttendanceOverview_ShouldLeaveUnpointedCoursesOutOfTheBars()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldLeaveUnpointedCoursesOutOfTheBars));
        SeedSession(dbContext, Yesterday(18), present: 10, template: NewTemplate("Power Cycle"));
        SeedSession(dbContext, Today(9), pending: 10, template: NewTemplate("Yoga Restore"));
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.CourseRates.Select(course => course.CourseName).ShouldBe(["Power Cycle"]);
    }

    /// <summary>
    /// "Absents à relancer": the members whose absences are piling up, worst
    /// first, with the last time they actually came.
    /// </summary>
    [Fact]
    public async Task GetAttendanceOverview_ShouldListTheMembersToChase()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldListTheMembersToChase));
        var chronic = new Member("Théo", "Garnier");
        var occasional = new Member("Sarah", "Cohen");
        var regular = new Member("Amina", "Benali");

        // Three sessions over the past fortnight. Théo missed all three, Sarah
        // missed two, Amina came to all of them.
        for (var day = 1; day <= 3; day++)
        {
            var session = SeedSession(dbContext, Yesterday(18).AddDays(-day), courseName: "Power Cycle");
            session.Registrations =
            [
                new Registration { Member = chronic, Status = AttendanceStatus.Absent },
                new Registration
                {
                    Member = occasional,
                    Status = day <= 2 ? AttendanceStatus.Absent : AttendanceStatus.Present,
                    CheckedInAt = day <= 2 ? null : Yesterday(18).AddDays(-day)
                },
                new Registration
                {
                    Member = regular,
                    Status = AttendanceStatus.Present,
                    CheckedInAt = Yesterday(18).AddDays(-day)
                }
            ];
            session.AttendanceClosedAt = Yesterday(19).AddDays(-day);
        }

        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        result.ToChase.Select(member => member.FullName).ShouldBe(["Théo Garnier", "Sarah Cohen"]);

        var worst = result.ToChase[0];
        worst.Missed.ShouldBe(3);
        worst.Booked.ShouldBe(3);
        worst.LastVisitOn.ShouldBeNull();

        result.ToChase[1].LastVisitOn.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAttendanceOverview_ShouldCountTodaysArrivalsAndTheWeeksNoShows()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetAttendanceOverview_ShouldCountTodaysArrivalsAndTheWeeksNoShows));
        SeedSession(dbContext, Today(7), present: 5, late: 2, absent: 1);
        await dbContext.SaveChangesAsync();

        var result = await Overview(dbContext);

        // A late arrival is somebody who is in the room.
        result.Kpis.PresentToday.ShouldBe(7);
        result.Kpis.NoShowsThisWeek.ShouldBe(1);
    }

    /// <summary>
    /// The two columns lot 1 shipped as "—". The list and the record read the
    /// same helper, so this pins both at once.
    /// </summary>
    [Fact]
    public async Task TheMembersListAndRecord_ShouldAgreeOnAssiduiteAndLastVisit()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(TheMembersListAndRecord_ShouldAgreeOnAssiduiteAndLastVisit));
        var member = new Member("Amina", "Benali");

        // Three pointed seats, one of them missed, the last visit a week ago.
        Seat(dbContext, member, Yesterday(18).AddDays(-14), AttendanceStatus.Present);
        Seat(dbContext, member, Yesterday(18).AddDays(-10), AttendanceStatus.Absent);
        Seat(dbContext, member, Yesterday(18).AddDays(-7), AttendanceStatus.Late);
        await dbContext.SaveChangesAsync();

        var row = (await new GetMembersQueryHandler(dbContext)
            .Handle(new GetMembersQuery(), CancellationToken.None)).Items.Single();

        var record = await new GetMemberDetailsPageQueryHandler(dbContext)
            .Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        row.AttendanceRate.ShouldBe(67);
        row.AttendanceRate.ShouldBe(record.Stats.AttendanceRate);

        row.LastVisitOn.ShouldBe(DateOnly.FromDateTime(Yesterday(18).AddDays(-7)));
        row.LastVisitOn.ShouldBe(record.Stats.LastVisitOn);
    }

    /// <summary>
    /// The rate looks back a quarter; the last visit does not. A member who last
    /// came four months ago has no recent assiduité to report but still has a
    /// last visit, and "—" on both would say they had never set foot in the
    /// place.
    /// </summary>
    [Fact]
    public async Task LastVisit_ShouldReachBeyondTheRateWindow()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(LastVisit_ShouldReachBeyondTheRateWindow));
        var member = new Member("Théo", "Garnier");
        var longAgo = DateTime.Today.AddDays(-(SessionStatistics.AttendanceWindowDays + 30)).AddHours(18);

        Seat(dbContext, member, longAgo, AttendanceStatus.Present);
        await dbContext.SaveChangesAsync();

        var row = (await new GetMembersQueryHandler(dbContext)
            .Handle(new GetMembersQuery(), CancellationToken.None)).Items.Single();

        row.AttendanceRate.ShouldBeNull();
        row.LastVisitOn.ShouldBe(DateOnly.FromDateTime(longAgo));
    }

    /// <summary>
    /// "Présences récentes" stops chipping everything « Passé » — a pointed seat
    /// says what was recorded.
    /// </summary>
    [Fact]
    public async Task TheMemberRecord_ShouldCarryTheVerdictOfEachPastSession()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(TheMemberRecord_ShouldCarryTheVerdictOfEachPastSession));
        var member = new Member("Sarah", "Cohen");

        Seat(dbContext, member, Yesterday(18), AttendanceStatus.Absent);
        Seat(dbContext, member, Today(9), AttendanceStatus.Pending);
        await dbContext.SaveChangesAsync();

        var record = await new GetMemberDetailsPageQueryHandler(dbContext)
            .Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        var pointed = record.PastSessions.Single(session => session.StartsAt == Yesterday(18));
        pointed.IsPointed.ShouldBeTrue();
        pointed.AttendanceStatus.ShouldBe(AttendanceStatus.Absent);

        var untouched = record.PastSessions.SingleOrDefault(session => session.StartsAt == Today(9));
        (untouched?.IsPointed ?? false).ShouldBeFalse();
    }

    private static void Seat(GymDbContext dbContext, Member member, DateTime startsAt, AttendanceStatus status)
    {
        var session = SeedSession(dbContext, startsAt);
        session.Registrations =
        [
            new Registration
            {
                Member = member,
                Status = status,
                CheckedInAt = AttendanceRules.CheckInFor(status, startsAt)
            }
        ];
    }

    private static async Task<TechXyz.GymXyz.Application.Models.AttendanceOverviewDto> Overview(GymDbContext dbContext) =>
        await new GetAttendanceOverviewQueryHandler(dbContext)
            .Handle(new GetAttendanceOverviewQuery(), CancellationToken.None);

    private static GetSessionRosterQueryHandler Roster(GymDbContext dbContext, params string[] roles)
    {
        ICurrentUserService user = new TestCurrentUserService(roles);

        return new GetSessionRosterQueryHandler(dbContext, user);
    }

    private static DateTime Today(int hour) => DateTime.Today.AddHours(hour);

    private static DateTime Yesterday(int hour) => Today(hour).AddDays(-1);

    private static CourseTemplate NewTemplate(string name) =>
        new(name)
        {
            Discipline = new Discipline($"{name} discipline"),
            Capacity = 24,
            DurationMinutes = 60
        };

    private static Session SeedSession(
        GymDbContext dbContext,
        DateTime startsAt,
        int present = 0,
        int late = 0,
        int absent = 0,
        int pending = 0,
        int waitlisted = 0,
        string courseName = "Cours",
        CourseTemplate? template = null)
    {
        var seats = new List<Registration>();
        Add(seats, present, AttendanceStatus.Present, startsAt);
        Add(seats, late, AttendanceStatus.Late, startsAt);
        Add(seats, absent, AttendanceStatus.Absent, startsAt);
        Add(seats, pending, AttendanceStatus.Pending, startsAt);

        for (var seat = 0; seat < waitlisted; seat++)
        {
            seats.Add(new Registration
            {
                Member = new Member($"Waiting{seat}", "Test"),
                IsWaitlisted = true
            });
        }

        var session = new Session
        {
            CourseTemplate = template ?? NewTemplate(courseName),
            Location = new Location("Studio C") { Kind = LocationKind.Studio, Capacity = 24 },
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            Capacity = 24,
            Registrations = seats
        };

        dbContext.Sessions.Add(session);

        return session;
    }

    private static void Add(List<Registration> seats, int count, AttendanceStatus status, DateTime startsAt)
    {
        for (var seat = 0; seat < count; seat++)
        {
            seats.Add(new Registration
            {
                Member = new Member($"{status}{seat}", "Test"),
                Status = status,
                CheckedInAt = AttendanceRules.CheckInFor(status, startsAt)
            });
        }
    }
}
