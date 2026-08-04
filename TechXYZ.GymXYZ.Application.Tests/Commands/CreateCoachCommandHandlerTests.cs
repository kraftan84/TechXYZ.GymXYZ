using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateCoachCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCoachAndNormalizeValues()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateCoachAndNormalizeValues));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());
        var command = new CreateCoachCommand(
            $"  {faker.Name.FirstName()}  ",
            $"  {faker.Name.LastName()}  ",
            $"  {faker.Internet.Email()}  ",
            "   ",
            $"  {faker.Address.StreetAddress()}  ",
            $" {faker.Address.ZipCode()} ",
            $" {faker.Address.City()} ",
            $" {faker.Address.Country()} ");

        var newId = await handler.Handle(command, CancellationToken.None);

        var created = dbContext.Coaches.Single(coach => coach.Id == newId);
        created.FirstName.ShouldBe(command.FirstName.Trim());
        created.LastName.ShouldBe(command.LastName.Trim());
        created.Email.ShouldBe(command.Email!.Trim());
        created.Phone.ShouldBeNull();
        created.Address.ShouldNotBeNull();
        created.Address!.Street.ShouldBe(command.Street!.Trim());
    }

    [Fact]
    public async Task Handle_ShouldWriteAvailabilityDisciplinesAndCertifications()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldWriteAvailabilityDisciplinesAndCertifications));
        dbContext.Gyms.Add(new Gym("GymXYZ"));

        var yoga = new Discipline("Yoga");
        var pilates = new Discipline("Pilates");
        dbContext.Disciplines.AddRange(yoga, pilates);
        await dbContext.SaveChangesAsync();

        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());

        var newId = await handler.Handle(new CreateCoachCommand(
            "Inès", "Ravel", null, null, null, null, null, null,
            roleLabel: "  Coach yoga & mobilité  ",
            bio: "  Relance les créneaux du matin.  ",
            joinedOn: DateOnly.FromDateTime(DateTime.Today).AddMonths(-25),
            awayUntil: null,
            availability: [true, true, false, true, true, true, true],
            // Pilates first: the pick order is the display order.
            disciplineIds: [pilates.Id, yoga.Id],
            certifications: ["  Yoga Alliance 300h  ", "   ", "Mobilité fonctionnelle · FRC"]),
            CancellationToken.None);

        var created = dbContext.Coaches.Single(coach => coach.Id == newId);
        created.RoleLabel.ShouldBe("Coach yoga & mobilité");
        created.Bio.ShouldBe("Relance les créneaux du matin.");
        created.AwayUntil.ShouldBeNull();
        created.AvailableOnWednesday.ShouldBeFalse();
        created.AvailableOnMonday.ShouldBeTrue();

        var disciplines = dbContext.CoachDisciplines
            .Where(link => link.CoachId == newId)
            .OrderBy(link => link.Rank)
            .Select(link => link.DisciplineId)
            .ToList();
        disciplines.ShouldBe([pilates.Id, yoga.Id]);

        var certifications = dbContext.CoachCertifications
            .Where(certification => certification.CoachId == newId)
            .OrderBy(certification => certification.Rank)
            .Select(certification => certification.Label)
            .ToList();
        certifications.ShouldBe(["Yoga Alliance 300h", "Mobilité fonctionnelle · FRC"]);
    }

    [Fact]
    public async Task Handle_ShouldLeaveANewCoachAvailableEveryDay_WhenNoAvailabilityIsGiven()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldLeaveANewCoachAvailableEveryDay_WhenNoAvailabilityIsGiven));
        dbContext.Gyms.Add(new Gym("GymXYZ"));
        await dbContext.SaveChangesAsync();

        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());
        var newId = await handler.Handle(
            new CreateCoachCommand("Karim", "Bouaziz", null, null, null, null, null, null),
            CancellationToken.None);

        var created = dbContext.Coaches.Single(coach => coach.Id == newId);
        created.AvailableOnMonday.ShouldBeTrue();
        created.AvailableOnSunday.ShouldBeTrue();
        created.JoinedOn.ShouldBe(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenADisciplineDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenADisciplineDoesNotExist));
        dbContext.Gyms.Add(new Gym("GymXYZ"));
        await dbContext.SaveChangesAsync();

        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(
            new CreateCoachCommand("Karim", "Bouaziz", null, null, null, null, null, null,
                disciplineIds: [999]),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenLastNameIsEmpty()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenLastNameIsEmpty));
        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());
        var command = new CreateCoachCommand(
            faker.Name.FirstName(),
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null);

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist));
        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateCoachCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            faker.Internet.Email(),
            null,
            null,
            null,
            null,
            null), CancellationToken.None));
    }
}
