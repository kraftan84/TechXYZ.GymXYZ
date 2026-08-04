using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateCoachCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateExistingCoach()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateExistingCoach));
        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName())
        {
            Email = faker.Internet.Email(),
            Phone = faker.Phone.PhoneNumber(),
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

        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var email = faker.Internet.Email();

        var updated = await handler.Handle(new UpdateCoachCommand(
            coach.Id,
            $" {firstName} ",
            $" {lastName} ",
            $" {email} ",
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        updated.ShouldBeTrue();

        var persisted = dbContext.Coaches.Single(candidate => candidate.Id == coach.Id);
        persisted.FirstName.ShouldBe(firstName);
        persisted.LastName.ShouldBe(lastName);
        persisted.Email.ShouldBe(email);
        persisted.Phone.ShouldBeNull();
        persisted.Address.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReplaceDisciplinesAndCertifications_AndKeepTheirOrder()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReplaceDisciplinesAndCertifications_AndKeepTheirOrder));

        var yoga = new Discipline("Yoga");
        var pilates = new Discipline("Pilates");
        var boxe = new Discipline("Boxe");
        dbContext.Disciplines.AddRange(yoga, pilates, boxe);

        var coach = new Coach("Inès", "Ravel");
        coach.AddDiscipline(yoga, 0);
        coach.AddDiscipline(pilates, 1);
        coach.AddCertification("Yoga Alliance 300h", 0);
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        // Yoga is kept but demoted, Pilates dropped, Boxe added.
        var updated = await handler.Handle(new UpdateCoachCommand(
            coach.Id, "Inès", "Ravel", null, null, null, null, null, null,
            disciplineIds: [boxe.Id, yoga.Id],
            certifications: ["PSC1 · premiers secours", "Yoga Alliance 300h"]),
            CancellationToken.None);

        updated.ShouldBeTrue();

        dbContext.CoachDisciplines
            .Where(link => link.CoachId == coach.Id)
            .OrderBy(link => link.Rank)
            .Select(link => link.DisciplineId)
            .ToList()
            .ShouldBe([boxe.Id, yoga.Id]);

        dbContext.CoachCertifications
            .Where(certification => certification.CoachId == coach.Id)
            .OrderBy(certification => certification.Rank)
            .Select(certification => certification.Label)
            .ToList()
            .ShouldBe(["PSC1 · premiers secours", "Yoga Alliance 300h"]);
    }

    [Fact]
    public async Task Handle_ShouldClearTheLeave_WhenNoDateIsGiven()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldClearTheLeave_WhenNoDateIsGiven));

        var coach = new Coach("Théo", "Garnier")
        {
            AwayUntil = DateOnly.FromDateTime(DateTime.Today).AddDays(11)
        };
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        var updated = await handler.Handle(new UpdateCoachCommand(
            coach.Id, "Théo", "Garnier", null, null, null, null, null, null),
            CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Coaches.Single(candidate => candidate.Id == coach.Id).AwayUntil.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldLeaveAvailabilityUntouched_WhenNoneIsGiven()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldLeaveAvailabilityUntouched_WhenNoneIsGiven));

        var coach = new Coach("Nora", "Lemoine") { AvailableOnMonday = true, AvailableOnSunday = false };
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        await handler.Handle(new UpdateCoachCommand(
            coach.Id, "Nora", "Lemoine", null, null, null, null, null, null),
            CancellationToken.None);

        var persisted = dbContext.Coaches.Single(candidate => candidate.Id == coach.Id);
        persisted.AvailableOnMonday.ShouldBeTrue();
        persisted.AvailableOnSunday.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCoachDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenCoachDoesNotExist));
        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        var updated = await handler.Handle(new UpdateCoachCommand(
            999,
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null,
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        updated.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        var handler = new UpdateCoachCommandHandler(dbContext, new UpdateCoachCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new UpdateCoachCommand(
            0,
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null,
            null,
            null,
            null,
            null,
            null), CancellationToken.None));
    }
}
