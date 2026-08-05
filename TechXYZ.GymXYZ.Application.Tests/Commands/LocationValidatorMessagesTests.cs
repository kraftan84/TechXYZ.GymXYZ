using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The validation messages reach the user through a toast, so they have to read
/// like the rest of the product: French, and never carrying a C# property name.
/// </summary>
public class LocationValidatorMessagesTests
{
    [Fact]
    public async Task CreateLocationValidator_ShouldNameFieldsInFrench()
    {
        var result = await new CreateLocationCommandValidator().ValidateAsync(
            new CreateLocationCommand(string.Empty, LocationKind.Studio, capacity: 0));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("Le nom du lieu"));
        messages.ShouldContain(message => message.Contains("La capacité doit être comprise"));
        messages.ShouldAllBe(message =>
            !message.Contains("Type Label") &&
            !message.Contains("Area Sqm") &&
            !message.Contains("Name"));
    }

    [Fact]
    public async Task UpdateLocationValidator_ShouldNameFieldsInFrench()
    {
        var result = await new UpdateLocationCommandValidator().ValidateAsync(
            new UpdateLocationCommand(
                id: 0, string.Empty, LocationKind.Studio, capacity: 20, areaSqm: 0m));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("L'identifiant"));
        messages.ShouldContain(message => message.Contains("La surface doit être comprise"));
        messages.ShouldAllBe(message =>
            !message.Contains("Area Sqm") &&
            !message.Contains("Fallback Location Id") &&
            !message.Contains("Name"));
    }

    /// <summary>
    /// The address of a session at the member's home is on the member record and
    /// changes with every session, so the venue must not carry one of its own.
    /// </summary>
    [Fact]
    public async Task CreateLocationValidator_ShouldRefuseAnAddressOnAHomeVenue()
    {
        var result = await new CreateLocationCommandValidator().ValidateAsync(
            new CreateLocationCommand(
                "À domicile", LocationKind.Home, capacity: 1, street: "14 rue de la Villette"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage.Contains("séance à domicile"));
    }
}
