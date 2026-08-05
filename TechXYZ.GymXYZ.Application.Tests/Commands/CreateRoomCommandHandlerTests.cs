using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateRoomInSite()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateRoomInSite));

        var site = new Site(faker.Address.City())
        {
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };
        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync();

        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        var roomName = faker.Commerce.ProductName();
        var roomId = await handler.Handle(new CreateRoomCommand($" {roomName} ", site.Id), CancellationToken.None);

        roomId.ShouldBeGreaterThan(0);
        dbContext.Rooms.Any(r => r.Id == roomId && r.Name == roomName).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSiteIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenSiteIdIsInvalid));
        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateRoomCommand(faker.Commerce.ProductName(), 0), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSiteDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenSiteDoesNotExist));
        var handler = new CreateRoomCommandHandler(dbContext, new CreateRoomCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateRoomCommand(faker.Commerce.ProductName(), 999), CancellationToken.None));
    }
}
