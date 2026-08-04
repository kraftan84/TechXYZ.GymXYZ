using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCourseTemplateCommandValidator : AbstractValidator<CreateCourseTemplateCommand>
{
    public CreateCourseTemplateCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(120)
            .WithName(CourseTemplateFieldNames.Name);

        RuleFor(command => command.DisciplineId)
            .GreaterThan(0)
            .WithName(CourseTemplateFieldNames.Discipline);

        RuleFor(command => command.DurationMinutes)
            .InclusiveBetween(CourseTemplateRules.MinimumDurationMinutes, CourseTemplateRules.MaximumDurationMinutes)
            .WithMessage(CourseTemplateRules.DurationMessage);

        RuleFor(command => command.Capacity)
            .InclusiveBetween(CourseTemplateRules.MinimumCapacity, CourseTemplateRules.MaximumCapacity)
            .WithMessage(CourseTemplateRules.CapacityMessage);

        RuleFor(command => command.Price)
            .GreaterThanOrEqualTo(0)
            .When(command => command.Price.HasValue)
            .WithName(CourseTemplateFieldNames.Price);

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithName(CourseTemplateFieldNames.Description);

        RuleFor(command => command.IconKey)
            .MaximumLength(50)
            .WithName(CourseTemplateFieldNames.IconKey);
    }
}
