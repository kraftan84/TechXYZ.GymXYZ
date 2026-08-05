using FluentValidation;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Description).MaximumLength(1000);
        RuleFor(command => command.CoachId).GreaterThan(0);
        RuleFor(command => command.LocationId).GreaterThan(0);
        RuleFor(command => command.ThemeId)
            .GreaterThan(0)
            .When(command => command.ThemeId.HasValue);
        RuleFor(command => command.EndDate)
            .GreaterThan(command => command.StartDate);
        RuleFor(command => command.MaxParticipants)
            .NotNull()
            .GreaterThan(0)
            .When(command => command.Type == LessonType.Collective);
    }
}
