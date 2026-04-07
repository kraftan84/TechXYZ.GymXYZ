using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteRoom()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldDeleteRoom));
        var room = new Room("Room 1");
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteRoomCommandHandler(dbContext, new DeleteRoomCommandValidator());

        var deleted = await handler.Handle(new DeleteRoomCommand(room.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Rooms.Any(r => r.Id == room.Id).ShouldBeFalse();
    }
}
