using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CreateMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateMemberAndNormalizeValues()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCreateMemberAndNormalizeValues));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());
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
    public async Task Handle_ShouldStoreJoinDateBirthDateAndNotes()
    {
        var faker = TestInfrastructure.Faker();
        var today = DateOnly.FromDateTime(DateTime.Today);

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldStoreJoinDateBirthDateAndNotes));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());
        var newId = await handler.Handle(new CreateMemberCommand(
            "Camille",
            "Durand",
            "camille.durand@gymxyz.fr",
            "06 22 11 90 04",
            null,
            null,
            null,
            null,
            joinedOn: today.AddMonths(-17),
            birthDate: today.AddYears(-29),
            notes: "  Préfère les cours du matin.  "), CancellationToken.None);

        var created = dbContext.Members.Single(member => member.Id == newId);
        created.JoinedOn.ShouldBe(today.AddMonths(-17));
        created.BirthDate.ShouldBe(today.AddYears(-29));
        created.Notes.ShouldBe("Préfère les cours du matin.");
    }

    [Fact]
    public async Task Handle_ShouldDefaultJoinDateToToday_WhenNotSupplied()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldDefaultJoinDateToToday_WhenNotSupplied));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());
        var newId = await handler.Handle(new CreateMemberCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null, null, null, null, null, null), CancellationToken.None);

        dbContext.Members.Single(member => member.Id == newId).JoinedOn
            .ShouldBe(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenBirthDateIsInTheFuture()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenBirthDateIsInTheFuture));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateMemberCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null, null, null, null, null, null,
            birthDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1))), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenJoinDateIsInTheFuture()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenJoinDateIsInTheFuture));
        dbContext.Gyms.Add(new Gym(faker.Company.CompanyName()));
        await dbContext.SaveChangesAsync();

        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateMemberCommand(
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null, null, null, null, null, null,
            joinedOn: DateOnly.FromDateTime(DateTime.Today.AddDays(1))), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenFirstNameIsEmpty()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenFirstNameIsEmpty));
        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());
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

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenDefaultGymDoesNotExist));
        var handler = new CreateMemberCommandHandler(dbContext, new CreateMemberCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new CreateMemberCommand(
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
