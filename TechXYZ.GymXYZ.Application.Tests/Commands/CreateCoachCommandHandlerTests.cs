using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateCoachCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCoachAndNormalizeValues()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateCoachAndNormalizeValues));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());
        var command = new CreateCoachCommand(
            $"  {faker.Name.FirstName()}  ",
            $"  {faker.Name.LastName()}  ",
            $"  {faker.Internet.Email()}  ",
            "   ",
            $"  {faker.Address.StreetAddress()}  ",
            $" {faker.Address.ZipCode()} ",
            $" {faker.Address.City()} ",
            $" {faker.Address.Country()} ");

        var newId = await handler.Handle(command, CancellationToken.None);

        var created = dbContext.Coaches.Single(coach => coach.Id == newId);
        created.FirstName.ShouldBe(command.FirstName.Trim());
        created.LastName.ShouldBe(command.LastName.Trim());
        created.Email.ShouldBe(command.Email!.Trim());
        created.Phone.ShouldBeNull();
        created.Address.ShouldNotBeNull();
        created.Address!.Street.ShouldBe(command.Street!.Trim());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenLastNameIsEmpty()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenLastNameIsEmpty));
        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());
        var command = new CreateCoachCommand(
            faker.Name.FirstName(),
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null);

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist));
        var handler = new CreateCoachCommandHandler(dbContext, new CreateCoachCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateCoachCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            faker.Internet.Email(),
            null,
            null,
            null,
            null,
            null), CancellationToken.None));
    }
}
