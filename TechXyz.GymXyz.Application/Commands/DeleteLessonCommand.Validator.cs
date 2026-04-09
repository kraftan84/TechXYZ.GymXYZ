using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
