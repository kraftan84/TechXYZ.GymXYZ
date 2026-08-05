using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMoveLocationAndRename()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldMoveLocationAndRename));

        var source = new Site("Source")
        {
            Address = new Address { Street = "S", ZipCode = "1", City = "A", Country = "FR" }
        };
        var target = new Site("Target")
        {
            Address = new Address { Street = "T", ZipCode = "2", City = "B", Country = "FR" }
        };
        var location = new Location("Old Name");
        source.AddLocation(location);

        dbContext.Sites.AddRange(source, target);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateLocationCommandHandler(dbContext, new UpdateLocationCommandValidator());

        var newName = faker.Commerce.ProductName();
        var updated = await handler.Handle(new UpdateLocationCommand(location.Id, $" {newName} ", target.Id), CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Locations.Single(r => r.Id == location.Id).Name.ShouldBe(newName);
    }
}
