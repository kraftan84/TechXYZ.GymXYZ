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

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new UpdateCoachCommandHandler(unitOfWork, new UpdateCoachCommandValidator());

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
    public async Task Handle_ShouldReturnFalse_WhenCoachDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenCoachDoesNotExist));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new UpdateCoachCommandHandler(unitOfWork, new UpdateCoachCommandValidator());

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
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new UpdateCoachCommandHandler(unitOfWork, new UpdateCoachCommandValidator());

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
