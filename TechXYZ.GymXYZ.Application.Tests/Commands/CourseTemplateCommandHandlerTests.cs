using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CourseTemplateCommandHandlerTests
{
    [Fact]
    public async Task Create_ShouldWriteTheTemplateAndItsCoachesInOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Create_ShouldWriteTheTemplateAndItsCoachesInOrder));

        var cycling = new Discipline("Cycling");
        dbContext.Disciplines.Add(cycling);

        var studioC = new Location("Studio C");
        dbContext.Locations.Add(studioC);

        var lea = new Coach("Léa", "Fontaine");
        var nora = new Coach("Nora", "Lemoine");
        dbContext.Coaches.AddRange(lea, nora);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCourseTemplateCommandHandler(dbContext, new CreateCourseTemplateCommandValidator());

        var newId = await handler.Handle(new CreateCourseTemplateCommand(
            "  Power Cycle  ",
            cycling.Id,
            durationMinutes: 45,
            capacity: 24,
            CourseLevel.Intermediate,
            CourseIntensity.High,
            defaultLocationId: studioC.Id,
            price: null,
            description: "  Séance de vélo indoor.  ",
            iconKey: "   ",
            coachIds: [lea.Id, nora.Id]), CancellationToken.None);

        var created = await dbContext.CourseTemplates
            .Include(template => template.Coaches!)
            .SingleAsync(template => template.Id == newId);

        created.Name.ShouldBe("Power Cycle");
        created.Description.ShouldBe("Séance de vélo indoor.");
        created.IconKey.ShouldBeNull();
        created.DefaultLocationId.ShouldBe(studioC.Id);
        created.Price.ShouldBeNull();
        created.Coaches!.OrderBy(link => link.Rank).Select(link => link.CoachId)
            .ShouldBe([lea.Id, nora.Id]);
    }

    [Fact]
    public async Task Create_ShouldRefuseAnUnknownDiscipline()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Create_ShouldRefuseAnUnknownDiscipline));

        var handler = new CreateCourseTemplateCommandHandler(dbContext, new CreateCourseTemplateCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateCourseTemplateCommand(
                "Power Cycle", disciplineId: 404, durationMinutes: 45, capacity: 24,
                CourseLevel.AllLevels, CourseIntensity.High),
            CancellationToken.None));
    }

    [Fact]
    public async Task Update_ShouldReplaceTheCoachesAndKeepThePickOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Update_ShouldReplaceTheCoachesAndKeepThePickOrder));

        var cycling = new Discipline("Cycling");
        dbContext.Disciplines.Add(cycling);

        var lea = new Coach("Léa", "Fontaine");
        var nora = new Coach("Nora", "Lemoine");
        var samir = new Coach("Samir", "El Amrani");
        dbContext.Coaches.AddRange(lea, nora, samir);

        var template = NewTemplate("Power Cycle", cycling);
        template.AddCoach(lea, 0);
        template.AddCoach(nora, 1);
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCourseTemplateCommandHandler(dbContext, new UpdateCourseTemplateCommandValidator());

        var updated = await handler.Handle(new UpdateCourseTemplateCommand(
            template.Id,
            "Power Cycle",
            cycling.Id,
            durationMinutes: 50,
            capacity: 20,
            CourseLevel.Beginner,
            CourseIntensity.Moderate,
            defaultLocationId: null,
            price: 45m,
            description: null,
            iconKey: null,
            coachIds: [samir.Id, lea.Id]), CancellationToken.None);

        updated.ShouldBeTrue();

        var reloaded = await dbContext.CourseTemplates
            .Include(candidate => candidate.Coaches!)
            .SingleAsync(candidate => candidate.Id == template.Id);

        reloaded.DurationMinutes.ShouldBe(50);
        reloaded.Capacity.ShouldBe(20);
        reloaded.Price.ShouldBe(45m);
        reloaded.Coaches!.OrderBy(link => link.Rank).Select(link => link.CoachId)
            .ShouldBe([samir.Id, lea.Id]);
    }

    /// <summary>
    /// A null price is not "unchanged": it is what the drawer's empty field
    /// means, and the catalogue shows it as "Inclus".
    /// </summary>
    [Fact]
    public async Task Update_ShouldClearThePriceWhenNoneIsGiven()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Update_ShouldClearThePriceWhenNoneIsGiven));

        var discipline = new Discipline("Coaching perso");
        dbContext.Disciplines.Add(discipline);

        var template = NewTemplate("Coaching Perso", discipline);
        template.Price = 45m;
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCourseTemplateCommandHandler(dbContext, new UpdateCourseTemplateCommandValidator());

        await handler.Handle(new UpdateCourseTemplateCommand(
            template.Id, "Coaching Perso", discipline.Id, 60, 1,
            CourseLevel.Custom, CourseIntensity.Private), CancellationToken.None);

        dbContext.CourseTemplates.Single(candidate => candidate.Id == template.Id).Price.ShouldBeNull();
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenTheTemplateIsArchived()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Update_ShouldReturnFalse_WhenTheTemplateIsArchived));

        var discipline = new Discipline("Yoga");
        dbContext.Disciplines.Add(discipline);

        var template = NewTemplate("Yoga Restore", discipline);
        template.IsActive = false;
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCourseTemplateCommandHandler(dbContext, new UpdateCourseTemplateCommandValidator());

        var updated = await handler.Handle(new UpdateCourseTemplateCommand(
            template.Id, "Yoga Restore", discipline.Id, 60, 20,
            CourseLevel.AllLevels, CourseIntensity.Gentle), CancellationToken.None);

        updated.ShouldBeFalse();
    }

    /// <summary>
    /// Re-ranking an existing link rather than dropping and recreating it is
    /// what keeps the unique (template, coach) index happy mid-save — and the
    /// index only exists on a relational provider.
    /// </summary>
    [Fact]
    public async Task Update_ShouldRerankWithoutViolatingTheUniqueIndex_OnSqlite()
    {
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var cycling = new Discipline("Cycling");
        dbContext.Disciplines.Add(cycling);

        var lea = new Coach("Léa", "Fontaine");
        var nora = new Coach("Nora", "Lemoine");
        dbContext.Coaches.AddRange(lea, nora);

        var template = NewTemplate("Power Cycle", cycling);
        template.AddCoach(lea, 0);
        template.AddCoach(nora, 1);
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCourseTemplateCommandHandler(dbContext, new UpdateCourseTemplateCommandValidator());

        // The same two coaches, the other way round.
        var updated = await handler.Handle(new UpdateCourseTemplateCommand(
            template.Id, "Power Cycle", cycling.Id, 60, 16,
            CourseLevel.AllLevels, CourseIntensity.Moderate,
            coachIds: [nora.Id, lea.Id]), CancellationToken.None);

        updated.ShouldBeTrue();

        var links = await dbContext.CourseTemplateCoaches
            .Where(link => link.CourseTemplateId == template.Id)
            .OrderBy(link => link.Rank)
            .ToListAsync();

        links.Select(link => link.CoachId).ShouldBe([nora.Id, lea.Id]);
        links.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Delete_ShouldArchiveRatherThanRemove()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Delete_ShouldArchiveRatherThanRemove));

        var discipline = new Discipline("Renforcement");
        dbContext.Disciplines.Add(discipline);

        var template = NewTemplate("Core Express", discipline);
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteCourseTemplateCommandHandler(dbContext, new DeleteCourseTemplateCommandValidator());

        (await handler.Handle(new DeleteCourseTemplateCommand(template.Id), CancellationToken.None))
            .ShouldBeTrue();

        var archived = dbContext.CourseTemplates.IgnoreQueryFilters()
            .Single(candidate => candidate.Id == template.Id);
        archived.IsActive.ShouldBeFalse();

        // A second archive finds nothing active to act on.
        (await handler.Handle(new DeleteCourseTemplateCommand(template.Id), CancellationToken.None))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task Duplicate_ShouldCopyTheSettingsAndTheCoachOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Duplicate_ShouldCopyTheSettingsAndTheCoachOrder));

        var coaching = new Discipline("Coaching perso");
        dbContext.Disciplines.Add(coaching);

        var studioC = new Location("Studio C");
        dbContext.Locations.Add(studioC);

        var samir = new Coach("Samir", "El Amrani");
        var karim = new Coach("Karim", "Bouaziz");
        dbContext.Coaches.AddRange(samir, karim);

        var template = NewTemplate("Coaching Perso", coaching, capacity: 1);
        template.DefaultLocation = studioC;
        template.Price = 45m;
        template.Level = CourseLevel.Custom;
        template.Intensity = CourseIntensity.Private;
        template.Description = "Séance individuelle.";
        template.AddCoach(samir, 0);
        template.AddCoach(karim, 1);
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new DuplicateCourseTemplateCommandHandler(
            dbContext, new DuplicateCourseTemplateCommandValidator());

        var copyId = await handler.Handle(
            new DuplicateCourseTemplateCommand(template.Id), CancellationToken.None);

        copyId.ShouldNotBeNull();
        copyId.ShouldNotBe(template.Id);

        var copy = await dbContext.CourseTemplates
            .Include(candidate => candidate.Coaches!)
            .SingleAsync(candidate => candidate.Id == copyId);

        copy.Name.ShouldBe("Coaching Perso (copie)");
        copy.Capacity.ShouldBe(1);
        copy.Price.ShouldBe(45m);
        copy.DefaultLocationId.ShouldBe(studioC.Id);
        copy.Level.ShouldBe(CourseLevel.Custom);
        copy.Intensity.ShouldBe(CourseIntensity.Private);
        copy.Description.ShouldBe("Séance individuelle.");
        copy.Coaches!.OrderBy(link => link.Rank).Select(link => link.CoachId)
            .ShouldBe([samir.Id, karim.Id]);
    }

    [Fact]
    public async Task Duplicate_ShouldReturnNull_WhenTheTemplateIsArchived()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Duplicate_ShouldReturnNull_WhenTheTemplateIsArchived));

        var discipline = new Discipline("Yoga");
        dbContext.Disciplines.Add(discipline);

        var template = NewTemplate("Yoga Restore", discipline);
        template.IsActive = false;
        dbContext.CourseTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        var handler = new DuplicateCourseTemplateCommandHandler(
            dbContext, new DuplicateCourseTemplateCommandValidator());

        (await handler.Handle(new DuplicateCourseTemplateCommand(template.Id), CancellationToken.None))
            .ShouldBeNull();
    }

    private static CourseTemplate NewTemplate(string name, Discipline discipline, int capacity = 16)
        => new(name)
        {
            Discipline = discipline,
            DurationMinutes = 60,
            Capacity = capacity,
            Level = CourseLevel.AllLevels,
            Intensity = CourseIntensity.Moderate
        };
}
