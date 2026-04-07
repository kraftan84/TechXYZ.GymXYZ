using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateMemberAndNormalizeValues()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateMemberAndNormalizeValues));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new CreateMemberCommandHandler(unitOfWork, new CreateMemberCommandValidator());
        var command = new CreateMemberCommand(
            $"  {faker.Name.FirstName()}  ",
            $"  {faker.Name.LastName()}  ",
            $"  {faker.Internet.Email()}  ",
            "   ",
            $"  {faker.Address.StreetAddress()}  ",
            $" {faker.Address.ZipCode()} ",
            $" {faker.Address.City()} ",
            $" {faker.Address.Country()} ");

        var newId = await handler.Handle(command, CancellationToken.None);

        var created = dbContext.Members.Single(member => member.Id == newId);
        created.FirstName.ShouldBe(command.FirstName.Trim());
        created.LastName.ShouldBe(command.LastName.Trim());
        created.Email.ShouldBe(command.Email!.Trim());
        created.Phone.ShouldBeNull();
        created.Address.ShouldNotBeNull();
        created.Address!.Street.ShouldBe(command.Street!.Trim());
        created.Address.ZipCode.ShouldBe(command.ZipCode!.Trim());
        created.Address.City.ShouldBe(command.City!.Trim());
        created.Address.Country.ShouldBe(command.Country!.Trim());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenFirstNameIsEmpty()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenFirstNameIsEmpty));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new CreateMemberCommandHandler(unitOfWork, new CreateMemberCommandValidator());
        var command = new CreateMemberCommand(
            string.Empty,
            "Martin",
            null,
            null,
            null,
            null,
            null,
            null);

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
