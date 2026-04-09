using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLessonThemeCommandValidator : AbstractValidator<DeleteLessonThemeCommand>
{
    public DeleteLessonThemeCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
