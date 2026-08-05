using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CourseTemplateQueriesHandlerTests
{
    [Fact]
    public async Task GetCourseTemplates_ShouldReturnSortedList_AndSkipArchivedOnes()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCourseTemplates_ShouldReturnSortedList_AndSkipArchivedOnes));

        var cycling = new Discipline("Cycling");
        dbContext.Disciplines.Add(cycling);
        dbContext.CourseTemplates.AddRange(
            NewTemplate("Yoga Restore", cycling),
            NewTemplate("Boxing Fundamentals", cycling),
            NewTemplate("Core Express", cycling, isActive: false));
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);

        var result = await handler.Handle(new GetCourseTemplatesQuery(), CancellationToken.None);

        result.Items.Select(item => item.Name).ShouldBe(["Boxing Fundamentals", "Yoga Restore"]);
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetCourseTemplates_ShouldFallBackToTheDisciplineIcon()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCourseTemplates_ShouldFallBackToTheDisciplineIcon));

        var boxe = new Discipline("Boxe") { IconKey = "shield" };
        dbContext.Disciplines.Add(boxe);

        var inherited = NewTemplate("Core Express", boxe);
        var overridden = NewTemplate("Boxing Fundamentals", boxe);
        overridden.IconKey = "target";

        dbContext.CourseTemplates.AddRange(inherited, overridden);
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCourseTemplatesQuery(), CancellationToken.None);

        result.Items.Single(item => item.Name == "Core Express").IconKey.ShouldBe("shield");
        result.Items.Single(item => item.Name == "Boxing Fundamentals").IconKey.ShouldBe("target");
    }

    /// <summary>
    /// The two chips are derived from the capacity alone — there is no stored
    /// type to keep in step with it.
    /// </summary>
    [Fact]
    public async Task GetCourseTemplates_ShouldFilterAndCountByFormat()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCourseTemplates_ShouldFilterAndCountByFormat));

        var discipline = new Discipline("Renforcement");
        dbContext.Disciplines.Add(discipline);
        dbContext.CourseTemplates.AddRange(
            NewTemplate("HIIT Blast", discipline, capacity: 16),
            NewTemplate("Power Cycle", discipline, capacity: 24),
            NewTemplate("Coaching Perso", discipline, capacity: 1));
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);

        var everyone = await handler.Handle(new GetCourseTemplatesQuery(), CancellationToken.None);
        everyone.TotalCount.ShouldBe(3);
        everyone.CollectiveCount.ShouldBe(2);
        everyone.PrivateCount.ShouldBe(1);

        var privates = await handler.Handle(
            new GetCourseTemplatesQuery { Format = CourseFormat.Private }, CancellationToken.None);
        privates.Items.Select(item => item.Name).ShouldBe(["Coaching Perso"]);
        privates.Items.Single().IsPrivate.ShouldBeTrue();

        var collectives = await handler.Handle(
            new GetCourseTemplatesQuery { Format = CourseFormat.Collective }, CancellationToken.None);
        collectives.Items.Select(item => item.Name).ShouldBe(["HIIT Blast", "Power Cycle"]);
    }

    [Fact]
    public async Task GetCourseTemplates_ShouldCountChipsOnTheSearchedSet()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCourseTemplates_ShouldCountChipsOnTheSearchedSet));

        var cycling = new Discipline("Cycling");
        var coaching = new Discipline("Coaching perso");
        dbContext.Disciplines.AddRange(cycling, coaching);
        dbContext.CourseTemplates.AddRange(
            NewTemplate("Power Cycle", cycling, capacity: 24),
            NewTemplate("Coaching Perso", coaching, capacity: 1));
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);

        var searched = await handler.Handle(
            new GetCourseTemplatesQuery { Search = "Cycle" }, CancellationToken.None);

        searched.TotalCount.ShouldBe(1);
        searched.CollectiveCount.ShouldBe(1);
        searched.PrivateCount.ShouldBe(0);
    }

    /// <summary>
    /// The catalogue projects a collection navigation and searches through
    /// another one; both are the kind of thing the in-memory provider forgives.
    /// </summary>
    [Fact]
    public async Task GetCourseTemplates_ShouldProjectCoachesAndCountFormats_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var cycling = new Discipline("Cycling") { IconKey = "target" };
        var coaching = new Discipline("Coaching perso") { IconKey = "user" };
        dbContext.Disciplines.AddRange(cycling, coaching);

        var lea = new Coach("Léa", "Fontaine") { RoleLabel = "Coach cycling" };
        var nora = new Coach("Nora", "Lemoine");
        var samir = new Coach("Samir", "El Amrani");
        dbContext.Coaches.AddRange(lea, nora, samir);

        var powerCycle = NewTemplate("Power Cycle", cycling, capacity: 24);
        powerCycle.AddCoach(lea, 0);
        powerCycle.AddCoach(nora, 1);

        var perso = NewTemplate("Coaching Perso", coaching, capacity: 1);
        perso.AddCoach(samir, 0);

        dbContext.CourseTemplates.AddRange(powerCycle, perso, NewTemplate("Core Express", cycling));
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);

        var everyone = await handler.Handle(new GetCourseTemplatesQuery(), CancellationToken.None);

        everyone.Items.Single(item => item.Name == "Power Cycle").Coaches
            .Select(coach => coach.LastName)
            .ShouldBe(["Fontaine", "Lemoine"]);
        everyone.Items.Single(item => item.Name == "Core Express").Coaches.ShouldBeEmpty();

        everyone.TotalCount.ShouldBe(3);
        everyone.CollectiveCount.ShouldBe(2);
        everyone.PrivateCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetCourseTemplates_ShouldSearchNameAndDiscipline_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var cycling = new Discipline("Cycling");
        var yoga = new Discipline("Yoga");
        dbContext.Disciplines.AddRange(cycling, yoga);
        dbContext.CourseTemplates.AddRange(
            NewTemplate("Power Cycle", cycling),
            NewTemplate("Yoga Restore", yoga));
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplatesQueryHandler(dbContext);

        var byName = await handler.Handle(
            new GetCourseTemplatesQuery { Search = "Restore" }, CancellationToken.None);
        byName.Items.Select(item => item.Name).ShouldBe(["Yoga Restore"]);

        var byDiscipline = await handler.Handle(
            new GetCourseTemplatesQuery { Search = "Cycling" }, CancellationToken.None);
        byDiscipline.Items.Select(item => item.Name).ShouldBe(["Power Cycle"]);
        byDiscipline.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetCourseTemplateDetails_ShouldReturnNull_WhenArchived()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetCourseTemplateDetails_ShouldReturnNull_WhenArchived));

        var discipline = new Discipline("Yoga");
        dbContext.Disciplines.Add(discipline);

        var archived = NewTemplate("Yoga Restore", discipline, isActive: false);
        dbContext.CourseTemplates.Add(archived);
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplateDetailsPageQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetCourseTemplateDetailsPageQuery(archived.Id), CancellationToken.None);

        result.ShouldBeNull();
    }

    /// <summary>
    /// On the in-memory provider anything compiles; this is the test that proves
    /// the record's projection actually reaches a real engine.
    /// </summary>
    [Fact]
    public async Task GetCourseTemplateDetails_ShouldTranslate_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var cycling = new Discipline("Cycling") { IconKey = "target", Tone = "warning" };
        dbContext.Disciplines.Add(cycling);

        var studioC = new Location("Studio C");
        dbContext.Locations.Add(studioC);

        var lea = new Coach("Léa", "Fontaine") { RoleLabel = "Coach cycling" };
        var nora = new Coach("Nora", "Lemoine");
        dbContext.Coaches.AddRange(lea, nora);

        var powerCycle = NewTemplate("Power Cycle", cycling, capacity: 24);
        powerCycle.DefaultLocation = studioC;
        powerCycle.DurationMinutes = 45;
        powerCycle.Level = CourseLevel.Intermediate;
        powerCycle.Intensity = CourseIntensity.High;
        powerCycle.Description = "Séance de vélo indoor rythmée par la musique.";
        powerCycle.AddCoach(lea, 0);
        powerCycle.AddCoach(nora, 1);

        dbContext.CourseTemplates.Add(powerCycle);
        await dbContext.SaveChangesAsync();

        var handler = new GetCourseTemplateDetailsPageQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetCourseTemplateDetailsPageQuery(powerCycle.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Name.ShouldBe("Power Cycle");
        result.DisciplineName.ShouldBe("Cycling");
        result.IconKey.ShouldBe("target");
        result.DefaultLocationName.ShouldBe("Studio C");
        result.DurationMinutes.ShouldBe(45);
        result.Level.ShouldBe(CourseLevel.Intermediate);
        result.Intensity.ShouldBe(CourseIntensity.High);
        result.IsPrivate.ShouldBeFalse();
        result.Coaches.Select(coach => coach.LastName).ShouldBe(["Fontaine", "Lemoine"]);

        // A course the planning has never run has no figures to show.
        result.Stats.ShouldBe(CourseTemplateStatsDto.Empty);
        result.NextSessions.ShouldBeEmpty();
        result.Price.ShouldBeNull();
    }

    private static CourseTemplate NewTemplate(
        string name,
        Discipline discipline,
        int capacity = 16,
        bool isActive = true)
        => new(name)
        {
            Discipline = discipline,
            DurationMinutes = 60,
            Capacity = capacity,
            Level = CourseLevel.AllLevels,
            Intensity = CourseIntensity.Moderate,
            IsActive = isActive
        };
}
