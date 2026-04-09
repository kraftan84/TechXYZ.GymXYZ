using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonThemeCommandValidator : AbstractValidator<CreateLessonThemeCommand>
{
    public CreateLessonThemeCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Description).MaximumLength(500);
    }
}
