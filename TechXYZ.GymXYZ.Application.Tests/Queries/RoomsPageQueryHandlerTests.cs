using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class RoomsPageQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDefaultGymWithSitesAndRooms()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnDefaultGymWithSitesAndRooms));

        var defaultGym = new Gym("Default Gym") { Id = 1 };
        var secondGym = new Gym("Other Gym") { Id = 2 };

        var site = new Site("Downtown")
        {
            Address = new Address
            {
                Street = "12 street",
                ZipCode = "75000",
                City = "Paris",
                Country = "France"
            }
        };
        site.AddRoom(new Room("Room A"));
        site.AddRoom(new Room("Room Inactive") { IsActive = false });

        var inactiveSite = new Site("Inactive")
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

        defaultGym.AddSite(site);
        defaultGym.AddSite(inactiveSite);

        dbContext.Gyms.AddRange(defaultGym, secondGym);
        await dbContext.SaveChangesAsync();

        var handler = new GetRoomsPageQueryHandler(dbContext);

        var result = await handler.Handle(new GetRoomsPageQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.GymName.ShouldBe("Default Gym");
        result.Sites.Count.ShouldBe(1);
        result.Rooms.Count.ShouldBe(1);
        result.Rooms[0].SiteName.ShouldBe("Downtown");
    }
}
