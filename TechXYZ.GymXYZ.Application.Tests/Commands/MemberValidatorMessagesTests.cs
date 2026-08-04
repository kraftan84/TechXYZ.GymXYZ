using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The validation messages reach the user through a toast, so they have to read
/// like the rest of the product: French, and never carrying a C# property name.
/// </summary>
public class MemberValidatorMessagesTests
{
    [Fact]
    public async Task CreateMemberValidator_ShouldNameFieldsInFrench()
    {
        var result = await new CreateMemberCommandValidator().ValidateAsync(
            new CreateMemberCommand(string.Empty, string.Empty, null, null, null, null, null, null));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("Le prénom"));
        messages.ShouldContain(message => message.Contains("Le nom"));
        messages.ShouldAllBe(message => !message.Contains("First Name") && !message.Contains("Last Name"));
    }

    [Fact]
    public async Task UpdateMemberValidator_ShouldNameFieldsInFrench()
    {
        var result = await new UpdateMemberCommandValidator().ValidateAsync(
            new UpdateMemberCommand(0, string.Empty, string.Empty, "pas-un-email", null, null, null, null, null));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("L'identifiant"));
        messages.ShouldContain(message => message.Contains("L'adresse e-mail"));
        messages.ShouldAllBe(message =>
            !message.Contains("First Name") &&
            !message.Contains("Last Name") &&
            !message.Contains("Email") &&
            !message.Contains("Id "));
    }

    [Fact]
    public async Task CreateMemberValidator_ShouldRejectAFutureBirthDateWithItsOwnMessage()
    {
        var result = await new CreateMemberCommandValidator().ValidateAsync(
            new CreateMemberCommand("Nadia", "Ferrand", null, null, null, null, null, null,
                birthDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1))));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("La date de naissance doit être dans le passé.");
    }
}
