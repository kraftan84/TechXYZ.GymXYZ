using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateLocationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateLocationInSite()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateLocationInSite));

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

        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        var locationName = faker.Commerce.ProductName();
        var locationId = await handler.Handle(new CreateLocationCommand($" {locationName} ", site.Id), CancellationToken.None);

        locationId.ShouldBeGreaterThan(0);
        dbContext.Locations.Any(r => r.Id == locationId && r.Name == locationName).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSiteIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenSiteIdIsInvalid));
        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateLocationCommand(faker.Commerce.ProductName(), 0), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSiteDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenSiteDoesNotExist));
        var handler = new CreateLocationCommandHandler(dbContext, new CreateLocationCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateLocationCommand(faker.Commerce.ProductName(), 999), CancellationToken.None));
    }
}
