using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class UpdateMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateExistingMember()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldUpdateExistingMember));
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Email = faker.Internet.Email(),
            Phone = faker.Phone.PhoneNumber(),
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateMemberCommandHandler(dbContext, new UpdateMemberCommandValidator());

        var updatedFirstName = faker.Name.FirstName();
        var updatedLastName = faker.Name.LastName();
        var updatedEmail = faker.Internet.Email();
        var updatedPhone = faker.Phone.PhoneNumber();
        var updatedStreet = faker.Address.StreetAddress();
        var updatedZipCode = faker.Address.ZipCode();
        var updatedCity = faker.Address.City();
        var updatedCountry = faker.Address.Country();

        var updated = await handler.Handle(new UpdateMemberCommand(
            member.Id,
            $"  {updatedFirstName} ",
            $"  {updatedLastName} ",
            $"  {updatedEmail} ",
            $" {updatedPhone} ",
            $" {updatedStreet} ",
            $" {updatedZipCode} ",
            $" {updatedCity} ",
            $" {updatedCountry} "), CancellationToken.None);

        updated.ShouldBeTrue();

        var persisted = dbContext.Members.Single(candidate => candidate.Id == member.Id);
        persisted.FirstName.ShouldBe(updatedFirstName);
        persisted.LastName.ShouldBe(updatedLastName);
        persisted.Email.ShouldBe(updatedEmail);
        persisted.Phone.ShouldBe(updatedPhone);
        persisted.Address.ShouldNotBeNull();
        persisted.Address!.Street.ShouldBe(updatedStreet);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenMemberDoesNotExist()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenMemberDoesNotExist));
        var handler = new UpdateMemberCommandHandler(dbContext, new UpdateMemberCommandValidator());

        var updated = await handler.Handle(new UpdateMemberCommand(
            404,
            faker.Name.FirstName(),
            faker.Name.LastName(),
            null,
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        updated.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        var handler = new UpdateMemberCommandHandler(dbContext, new UpdateMemberCommandValidator());

        await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(new UpdateMemberCommand(
                0,
                faker.Name.FirstName(),
                faker.Name.LastName(),
                null,
                null,
                null,
                null,
                null,
                null), CancellationToken.None));
    }
}
