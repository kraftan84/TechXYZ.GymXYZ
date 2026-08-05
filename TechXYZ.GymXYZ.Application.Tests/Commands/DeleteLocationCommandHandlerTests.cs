using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteLocation()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteLocation));
        var location = new Location("Location 1");
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteLocationCommandHandler(dbContext, new DeleteLocationCommandValidator());

        var deleted = await handler.Handle(new DeleteLocationCommand(location.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Locations.Single(r => r.Id == location.Id).IsActive.ShouldBeFalse();
    }
}
