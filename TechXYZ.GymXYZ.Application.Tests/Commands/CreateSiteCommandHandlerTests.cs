using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateSiteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSiteInDefaultGym()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateSiteInDefaultGym));

        var gym = new Gym(faker.Company.CompanyName());
        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();

        var handler = new CreateSiteCommandHandler(dbContext, new CreateSiteCommandValidator());

        var command = new CreateSiteCommand(
            $" {faker.Address.City()} ",
            $" {faker.Address.StreetAddress()} ",
            $" {faker.Address.ZipCode()} ",
            $" {faker.Address.City()} ",
            $" {faker.Address.Country()} ");

        var createdId = await handler.Handle(command, CancellationToken.None);

        var site = dbContext.Sites.Single(l => l.Id == createdId);
        site.Name.ShouldBe(command.Name.Trim());
        site.Address.ShouldNotBeNull();
        site.Address.Street.ShouldBe(command.Street.Trim());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenNameIsEmpty));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateSiteCommandHandler(dbContext, new CreateSiteCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateSiteCommand(
            string.Empty,
            faker.Address.StreetAddress(),
            faker.Address.ZipCode(),
            faker.Address.City(),
            faker.Address.Country()), CancellationToken.None));
    }
}
