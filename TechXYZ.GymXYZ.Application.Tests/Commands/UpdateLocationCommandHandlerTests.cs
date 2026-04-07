using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateLocation()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateLocation));

        var location = new Location(faker.Address.City())
        {
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };

        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new UpdateLocationCommandHandler(unitOfWork, new UpdateLocationCommandValidator());

        var updatedName = faker.Address.City();
        var updatedStreet = faker.Address.StreetAddress();
        var updated = await handler.Handle(new UpdateLocationCommand(
            location.Id,
            $" {updatedName} ",
            $" {updatedStreet} ",
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Locations.Single(l => l.Id == location.Id).Name.ShouldBe(updatedName);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new UpdateLocationCommandHandler(unitOfWork, new UpdateLocationCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new UpdateLocationCommand(
            0,
            faker.Address.City(),
            faker.Address.StreetAddress(),
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None));
    }
}
