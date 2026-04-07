using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateRoomInLocation()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateRoomInLocation));

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

        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        var roomName = faker.Commerce.ProductName();
        var roomId = await handler.Handle(new CreateRoomCommand($" {roomName} ", location.Id), CancellationToken.None);

        roomId.ShouldBeGreaterThan(0);
        dbContext.Rooms.Any(r => r.Id == roomId && r.Name == roomName).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenLocationIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenLocationIdIsInvalid));
        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateRoomCommand(faker.Commerce.ProductName(), 0), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenLocationDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenLocationDoesNotExist));
        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateRoomCommand(faker.Commerce.ProductName(), 999), CancellationToken.None));
    }
}
