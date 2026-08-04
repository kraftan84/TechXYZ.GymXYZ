using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCourseTemplateCommandValidator : AbstractValidator<DeleteCourseTemplateCommand>
{
    public DeleteCourseTemplateCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithName(CourseTemplateFieldNames.Id);
    }
}
