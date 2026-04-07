using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteLocationAndItsRooms()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldDeleteLocationAndItsRooms));

        var location = new Location(faker.Address.City())
        {
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            },
            Rooms = [new Room("Room A"), new Room("Room B")]
        };

        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteLocationCommandHandler(dbContext, new DeleteLocationCommandValidator());

        var deleted = await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Locations.Any(l => l.Id == location.Id).ShouldBeFalse();
        dbContext.Rooms.Any().ShouldBeFalse();
    }
}
