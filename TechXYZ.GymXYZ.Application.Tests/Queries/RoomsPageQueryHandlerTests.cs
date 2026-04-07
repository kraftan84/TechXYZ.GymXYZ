using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class RoomsPageQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDefaultGymWithLocationsAndRooms()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnDefaultGymWithLocationsAndRooms));

        var defaultGym = new Gym("Default Gym") { Id = 1 };
        var secondGym = new Gym("Other Gym") { Id = 2 };

        var location = new Location("Downtown")
        {
            Address = new Address
            {
                Street = "12 street",
                ZipCode = "75000",
                City = "Paris",
                Country = "France"
            }
        };
        location.AddRoom(new Room("Room A"));
        defaultGym.AddLocation(location);

        dbContext.Gyms.AddRange(defaultGym, secondGym);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new GetRoomsPageQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetRoomsPageQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.GymName.ShouldBe("Default Gym");
        result.Locations.Count.ShouldBe(1);
        result.Rooms.Count.ShouldBe(1);
        result.Rooms[0].LocationName.ShouldBe("Downtown");
    }
}
