using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateCoachCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCoachAndNormalizeValues()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateCoachAndNormalizeValues));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new CreateCoachCommandHandler(unitOfWork, new CreateCoachCommandValidator());
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
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new CreateCoachCommandHandler(unitOfWork, new CreateCoachCommandValidator());
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
}
