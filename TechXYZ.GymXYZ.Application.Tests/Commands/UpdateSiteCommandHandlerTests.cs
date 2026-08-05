using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateSiteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateSite()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateSite));

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

        var handler = new UpdateSiteCommandHandler(dbContext, new UpdateSiteCommandValidator());

        var updatedName = faker.Address.City();
        var updatedStreet = faker.Address.StreetAddress();
        var updated = await handler.Handle(new UpdateSiteCommand(
            site.Id,
            $" {updatedName} ",
            $" {updatedStreet} ",
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None);

        updated.ShouldBeTrue();
        dbContext.Sites.Single(l => l.Id == site.Id).Name.ShouldBe(updatedName);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        var handler = new UpdateSiteCommandHandler(dbContext, new UpdateSiteCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new UpdateSiteCommand(
            0,
            faker.Address.City(),
            faker.Address.StreetAddress(),
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None));
    }
}
