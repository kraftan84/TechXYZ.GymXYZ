using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLessonThemeCommandValidator : AbstractValidator<UpdateLessonThemeCommand>
{
    public UpdateLessonThemeCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Description).MaximumLength(500);
    }
}
