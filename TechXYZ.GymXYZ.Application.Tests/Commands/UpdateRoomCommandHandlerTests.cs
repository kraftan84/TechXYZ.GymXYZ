using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMoveRoomAndRename()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldMoveRoomAndRename));

        var source = new Site("Source")
        {
            Address = new Address { Street = "S", ZipCode = "1", City = "A", Country = "FR" }
        };
        var target = new Site("Target")
        {
            Address = new Address { Street = "T", ZipCode = "2", City = "B", Country = "FR" }
        };
        var room = new Room("Old Name");
        source.AddRoom(room);

        dbContext.Sites.AddRange(source, target);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateRoomCommandHandler(dbContext, new UpdateRoomCommandValidator());

        var newName = faker.Commerce.ProductName();
        var updated = await handler.Handle(new UpdateRoomCommand(room.Id, $" {newName} ", target.Id), CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Rooms.Single(r => r.Id == room.Id).Name.ShouldBe(newName);
    }
}
