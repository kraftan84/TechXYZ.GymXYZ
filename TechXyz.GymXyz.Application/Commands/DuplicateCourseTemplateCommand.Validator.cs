using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DuplicateCourseTemplateCommandValidator : AbstractValidator<DuplicateCourseTemplateCommand>
{
    public DuplicateCourseTemplateCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithName(CourseTemplateFieldNames.Id);
    }
}
