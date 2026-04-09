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
        location.AddRoom(new Room("Room Inactive") { IsActive = false });

        var inactiveLocation = new Location("Inactive")
        {
            IsActive = false,
            Address = new Address
            {
                Street = "Other street",
                ZipCode = "69000",
                City = "Lyon",
                Country = "France"
            }
        };

        defaultGym.AddLocation(location);
        defaultGym.AddLocation(inactiveLocation);

        dbContext.Gyms.AddRange(defaultGym, secondGym);
        await dbContext.SaveChangesAsync();

        var handler = new GetRoomsPageQueryHandler(dbContext);

        var result = await handler.Handle(new GetRoomsPageQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.GymName.ShouldBe("Default Gym");
        result.Locations.Count.ShouldBe(1);
        result.Rooms.Count.ShouldBe(1);
        result.Rooms[0].LocationName.ShouldBe("Downtown");
    }
}
