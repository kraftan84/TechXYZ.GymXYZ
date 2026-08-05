using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteSiteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteSiteAndItsLocations()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteSiteAndItsLocations));

        var site = new Site(faker.Address.City())
        {
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            },
            Locations = [new Location("Location A"), new Location("Location B")]
        };

        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteSiteCommandHandler(dbContext, new DeleteSiteCommandValidator());

        var deleted = await handler.Handle(new DeleteSiteCommand(site.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Sites.Single(l => l.Id == site.Id).IsActive.ShouldBeFalse();
        dbContext.Locations.All(location => location.IsActive == false).ShouldBeTrue();
    }
}
