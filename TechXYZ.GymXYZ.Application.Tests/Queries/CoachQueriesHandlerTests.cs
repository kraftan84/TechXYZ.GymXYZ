using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CoachQueriesHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public async Task GetCoaches_ShouldReturnSortedList_AndSkipInactiveOnes()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldReturnSortedList_AndSkipInactiveOnes));
        dbContext.Coaches.AddRange(
            new Coach(faker.Name.FirstName(), "Zulu"),
            new Coach(faker.Name.FirstName(), "Alpha"),
            new Coach(faker.Name.FirstName(), "Omega") { IsActive = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);

        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        result.Items.Count.ShouldBe(2);
        result.Items[0].LastName.ShouldBe("Alpha");
        result.Items[1].LastName.ShouldBe("Zulu");
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetCoaches_ShouldMatchOnNameRoleAndDiscipline()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldMatchOnNameRoleAndDiscipline));

        var cycling = new Discipline("Cycling");
        var yoga = new Discipline("Yoga");
        dbContext.Disciplines.AddRange(cycling, yoga);

        var lea = new Coach("Léa", "Fontaine") { RoleLabel = "Coach cycling" };
        lea.AddDiscipline(cycling, 0);

        var ines = new Coach("Inès", "Ravel") { RoleLabel = "Coach yoga & mobilité" };
        ines.AddDiscipline(yoga, 0);

        dbContext.Coaches.AddRange(lea, ines);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);

        var byName = await handler.Handle(new GetCoachesQuery { Search = "Ravel" }, CancellationToken.None);
        byName.Items.Select(item => item.LastName).ShouldBe(["Ravel"]);

        var byDiscipline = await handler.Handle(new GetCoachesQuery { Search = "Cycling" }, CancellationToken.None);
        byDiscipline.Items.Select(item => item.LastName).ShouldBe(["Fontaine"]);
    }

    [Fact]
    public async Task GetCoaches_ShouldCountChipsOnTheSearchedSet()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldCountChipsOnTheSearchedSet));

        dbContext.Coaches.AddRange(
            new Coach("Nora", "Lemoine"),
            new Coach("Théo", "Garnier") { AwayUntil = Today.AddDays(11) },
            new Coach("Karim", "Bouaziz"));
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);

        var everyone = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);
        everyone.TotalCount.ShouldBe(3);
        everyone.AvailableCount.ShouldBe(2);
        everyone.AwayCount.ShouldBe(1);

        // The counts follow the search: a chip never offers a number the grid
        // cannot produce.
        var searched = await handler.Handle(new GetCoachesQuery { Search = "Garnier" }, CancellationToken.None);
        searched.TotalCount.ShouldBe(1);
        searched.AvailableCount.ShouldBe(0);
        searched.AwayCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetCoaches_ShouldReturnDisciplinesInDisplayOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldReturnDisciplinesInDisplayOrder));

        var pilates = new Discipline("Pilates");
        var yoga = new Discipline("Yoga");
        var hiit = new Discipline("HIIT");
        dbContext.Disciplines.AddRange(pilates, yoga, hiit);

        // Added out of order on purpose: the rank decides, not the insertion.
        var nora = new Coach("Nora", "Lemoine");
        nora.AddDiscipline(hiit, 2);
        nora.AddDiscipline(pilates, 0);
        nora.AddDiscipline(yoga, 1);

        dbContext.Coaches.Add(nora);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        result.Items[0].Disciplines.ShouldBe(["Pilates", "Yoga", "HIIT"]);
    }

    [Fact]
    public async Task GetCoaches_ShouldLeaveThePlanningFiguresUnset()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldLeaveThePlanningFiguresUnset));
        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        // Sessions land at lot 5: these read "—" on screen rather than a guess.
        result.Items[0].ClassesPerWeek.ShouldBeNull();
        result.Items[0].FillRate.ShouldBeNull();
    }

    [Fact]
    public async Task GetCoaches_ShouldOnlyReturnTheAmbientTenant()
    {
        var tenantContext = new TestTenantContext(TestInfrastructure.DefaultTenantId);
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCoaches_ShouldOnlyReturnTheAmbientTenant), tenantContext);

        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        await dbContext.SaveChangesAsync();

        using (tenantContext.UseTenant(TestInfrastructure.DefaultTenantId + 1))
        {
            dbContext.Coaches.Add(new Coach("Autre", "Client"));
            await dbContext.SaveChangesAsync();
        }

        var handler = new GetCoachesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].LastName.ShouldBe("Lemoine");
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetCoachDetails_ShouldReturnNull_WhenNotFoundOrInactive()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachDetails_ShouldReturnNull_WhenNotFoundOrInactive));

        var retired = new Coach("Ancien", "Coach") { IsActive = false };
        dbContext.Coaches.Add(retired);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachDetailsPageQueryHandler(dbContext);

        (await handler.Handle(new GetCoachDetailsPageQuery(12345), CancellationToken.None)).ShouldBeNull();
        (await handler.Handle(new GetCoachDetailsPageQuery(retired.Id), CancellationToken.None)).ShouldBeNull();
    }

    [Fact]
    public async Task GetCoachDetails_ShouldCarryTheWholeRecord()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachDetails_ShouldCarryTheWholeRecord));

        var boxe = new Discipline("Boxe");
        var cardio = new Discipline("Cardio");
        dbContext.Disciplines.AddRange(boxe, cardio);

        var theo = new Coach("Théo", "Garnier")
        {
            RoleLabel = "Coach boxe & cardio",
            Email = "theo.garnier@gymxyz.fr",
            Bio = "Ancien compétiteur amateur.",
            JoinedOn = Today.AddMonths(-39),
            AwayUntil = Today.AddDays(11),
            AvailableOnSaturday = true,
            AvailableOnSunday = true
        };
        theo.AddDiscipline(boxe, 0);
        theo.AddDiscipline(cardio, 1);
        theo.AddCertification("PSC1 · premiers secours", 1);
        theo.AddCertification("BPJEPS — Boxe anglaise", 0);

        dbContext.Coaches.Add(theo);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachDetailsPageQuery(theo.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.FullName.ShouldBe("Théo Garnier");
        result.Status.ShouldBe(CoachStatus.Away);
        result.Disciplines.Select(discipline => discipline.Name).ShouldBe(["Boxe", "Cardio"]);
        result.Certifications.ShouldBe(["BPJEPS — Boxe anglaise", "PSC1 · premiers secours"]);
        result.Availability.Days.ShouldBe([false, false, false, false, false, true, true]);
        result.Availability.AvailableDayCount.ShouldBe(2);
        result.HasAccount.ShouldBeFalse();

        // Everything below comes from the planning and the attendance sheets.
        result.WeekSessions.ShouldBeEmpty();
        result.Stats.ShouldBe(CoachStatsDto.Empty);
    }

    [Fact]
    public async Task GetCoaches_ShouldProjectAndSearchDisciplines_OnSqlite()
    {
        // The grid projects a collection navigation and searches through it;
        // both are the kind of thing the in-memory provider forgives.
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var cycling = new Discipline("Cycling");
        var cardio = new Discipline("Cardio");
        dbContext.Disciplines.AddRange(cycling, cardio);

        var lea = new Coach("Léa", "Fontaine");
        lea.AddDiscipline(cycling, 0);
        lea.AddDiscipline(cardio, 1);

        dbContext.Coaches.AddRange(lea, new Coach("Karim", "Bouaziz"));
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachesQueryHandler(dbContext);

        var everyone = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);
        everyone.Items.Single(item => item.LastName == "Fontaine").Disciplines
            .ShouldBe(["Cycling", "Cardio"]);
        everyone.Items.Single(item => item.LastName == "Bouaziz").Disciplines.ShouldBeEmpty();

        var searched = await handler.Handle(new GetCoachesQuery { Search = "Cycling" }, CancellationToken.None);
        searched.Items.Select(item => item.LastName).ShouldBe(["Fontaine"]);
        searched.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetCoachDetails_ShouldTranslate_OnSqlite()
    {
        // The record projects a nested availability, two ordered collections and
        // an empty week. On the in-memory provider anything compiles; this is
        // the test that proves the query actually reaches a real engine.
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var yoga = new Discipline("Yoga");
        dbContext.Disciplines.Add(yoga);

        var coach = new Coach("Inès", "Ravel")
        {
            RoleLabel = "Coach yoga & mobilité",
            AvailableOnMonday = true
        };
        coach.AddDiscipline(yoga, 0);
        coach.AddCertification("Yoga Alliance 300h", 0);

        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachDetailsPageQueryHandler(dbContext);
        var result = await handler.Handle(new GetCoachDetailsPageQuery(coach.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Disciplines.Select(discipline => discipline.Name).ShouldBe(["Yoga"]);
        result.Certifications.ShouldBe(["Yoga Alliance 300h"]);
        result.Availability.Days.ShouldBe([true, false, false, false, false, false, false]);
        result.Status.ShouldBe(CoachStatus.Available);
        result.WeekSessions.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCoachById_ShouldReturnNull_WhenNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachById_ShouldReturnNull_WhenNotFound));
        var handler = new GetCoachByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetCoachByIdQuery(12345), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCoachById_ShouldReturnCoachDto_WhenFound()
    {
        var faker = TestInfrastructure.Faker();
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var email = faker.Internet.Email();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachById_ShouldReturnCoachDto_WhenFound));

        var coach = new Coach(firstName, lastName)
        {
            Email = email,
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };

        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new GetCoachByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetCoachByIdQuery(coach.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(coach.Id);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
        result.Email.ShouldBe(email);
        result.Address.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetDisciplines_ShouldReturnActiveOnes_SortedByName()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetDisciplines_ShouldReturnActiveOnes_SortedByName));

        dbContext.Disciplines.AddRange(
            new Discipline("Yoga") { IconKey = "sparkles", Tone = "success" },
            new Discipline("Boxe"),
            new Discipline("Aquagym") { IsActive = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetDisciplinesQueryHandler(dbContext);
        var result = await handler.Handle(new GetDisciplinesQuery(), CancellationToken.None);

        result.Select(discipline => discipline.Name).ShouldBe(["Boxe", "Yoga"]);
        result[1].IconKey.ShouldBe("sparkles");
    }
}
