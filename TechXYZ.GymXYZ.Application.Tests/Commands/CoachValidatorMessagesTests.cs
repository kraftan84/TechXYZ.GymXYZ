using Shouldly;
using TechXyz.GymXyz.Application.Commands;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The validation messages reach the user through a toast, so they have to read
/// like the rest of the product: French, and never carrying a C# property name.
/// </summary>
public class CoachValidatorMessagesTests
{
    [Fact]
    public async Task CreateCoachValidator_ShouldNameFieldsInFrench()
    {
        var result = await new CreateCoachCommandValidator().ValidateAsync(
            new CreateCoachCommand(string.Empty, string.Empty, null, null, null, null, null, null));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("Le prénom"));
        messages.ShouldContain(message => message.Contains("Le nom"));
        messages.ShouldAllBe(message => !message.Contains("First Name") && !message.Contains("Last Name"));
    }

    [Fact]
    public async Task UpdateCoachValidator_ShouldNameFieldsInFrench()
    {
        var result = await new UpdateCoachCommandValidator().ValidateAsync(
            new UpdateCoachCommand(0, string.Empty, string.Empty, "pas-un-email", null, null, null, null, null));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("L'identifiant"));
        messages.ShouldContain(message => message.Contains("L'adresse e-mail"));
        messages.ShouldAllBe(message =>
            !message.Contains("Role Label") &&
            !message.Contains("First Name") &&
            !message.Contains("Last Name") &&
            !message.Contains("Email"));
    }

    [Fact]
    public async Task CreateCoachValidator_ShouldRejectAnIncompleteAvailability()
    {
        var result = await new CreateCoachCommandValidator().ValidateAsync(
            new CreateCoachCommand("Nora", "Lemoine", null, null, null, null, null, null,
                availability: [true, true, true]));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("La disponibilité doit couvrir les sept jours de la semaine.");
    }

    [Fact]
    public async Task CreateCoachValidator_ShouldRejectAFutureJoiningDateWithItsOwnMessage()
    {
        var result = await new CreateCoachCommandValidator().ValidateAsync(
            new CreateCoachCommand("Nora", "Lemoine", null, null, null, null, null, null,
                joinedOn: DateOnly.FromDateTime(DateTime.Today.AddDays(1))));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("La date d'arrivée ne peut pas être dans le futur.");
    }
}
