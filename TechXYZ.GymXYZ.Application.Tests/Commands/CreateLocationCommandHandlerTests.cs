using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateLocationInDefaultGym()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateLocationInDefaultGym));

        var gym = new Gym(faker.Company.CompanyName());
        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new CreateLocationCommandHandler(unitOfWork, new CreateLocationCommandValidator());

        var command = new CreateLocationCommand(
            $" {faker.Address.City()} ",
            $" {faker.Address.StreetAddress()} ",
            $" {faker.Address.ZipCode()} ",
            $" {faker.Address.City()} ",
            $" {faker.Address.Country()} ");

        var createdId = await handler.Handle(command, CancellationToken.None);

        var location = dbContext.Locations.Single(l => l.Id == createdId);
        location.Name.ShouldBe(command.Name.Trim());
        location.Address.ShouldNotBeNull();
        location.Address.Street.ShouldBe(command.Street.Trim());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenNameIsEmpty));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new CreateLocationCommandHandler(unitOfWork, new CreateLocationCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateLocationCommand(
            string.Empty,
            faker.Address.StreetAddress(),
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None));
    }
}
