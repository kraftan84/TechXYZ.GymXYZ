using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The validation messages reach the user through a toast, so they have to read
/// like the rest of the product: French, and never carrying a C# property name.
/// </summary>
public class CourseTemplateValidatorMessagesTests
{
    [Fact]
    public async Task CreateCourseTemplateValidator_ShouldNameFieldsInFrench()
    {
        var result = await new CreateCourseTemplateCommandValidator().ValidateAsync(
            new CreateCourseTemplateCommand(
                string.Empty, disciplineId: 0, durationMinutes: 0, capacity: 0,
                CourseLevel.AllLevels, CourseIntensity.Moderate));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("Le nom du cours"));
        messages.ShouldContain(message => message.Contains("La discipline"));
        messages.ShouldAllBe(message =>
            !message.Contains("Duration Minutes") &&
            !message.Contains("Discipline Id") &&
            !message.Contains("Name"));
    }

    [Fact]
    public async Task UpdateCourseTemplateValidator_ShouldNameFieldsInFrench()
    {
        var result = await new UpdateCourseTemplateCommandValidator().ValidateAsync(
            new UpdateCourseTemplateCommand(
                id: 0, string.Empty, disciplineId: 0, durationMinutes: 60, capacity: 16,
                CourseLevel.AllLevels, CourseIntensity.Moderate, price: -5m));

        result.IsValid.ShouldBeFalse();

        var messages = result.Errors.Select(error => error.ErrorMessage).ToList();
        messages.ShouldContain(message => message.Contains("L'identifiant"));
        messages.ShouldContain(message => message.Contains("Le tarif"));
        messages.ShouldAllBe(message =>
            !message.Contains("Discipline Id") &&
            !message.Contains("Price") &&
            !message.Contains("Name"));
    }

    [Fact]
    public async Task CreateCourseTemplateValidator_ShouldRejectAnOutOfRangeDurationWithItsOwnMessage()
    {
        var result = await new CreateCourseTemplateCommandValidator().ValidateAsync(
            new CreateCourseTemplateCommand(
                "Power Cycle", disciplineId: 1, durationMinutes: 600, capacity: 24,
                CourseLevel.AllLevels, CourseIntensity.High));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("La durée doit être comprise entre 5 et 300 minutes.");
    }

    /// <summary>One seat is the private lesson, so zero is the only rejected floor.</summary>
    [Fact]
    public async Task CreateCourseTemplateValidator_ShouldAcceptASingleSeatAndRejectNone()
    {
        var validator = new CreateCourseTemplateCommandValidator();

        var privateLesson = await validator.ValidateAsync(new CreateCourseTemplateCommand(
            "Coaching Perso", disciplineId: 1, durationMinutes: 60, capacity: 1,
            CourseLevel.Custom, CourseIntensity.Private));
        privateLesson.IsValid.ShouldBeTrue();

        var seatless = await validator.ValidateAsync(new CreateCourseTemplateCommand(
            "Coaching Perso", disciplineId: 1, durationMinutes: 60, capacity: 0,
            CourseLevel.Custom, CourseIntensity.Private));
        seatless.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("La capacité doit être comprise entre 1 et 200 places.");
    }
}
