using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(command => command.CourseTemplateId)
            .GreaterThan(0)
            .WithName(SessionFieldNames.Course);

        RuleFor(command => command.LocationId)
            .GreaterThan(0)
            .WithName(SessionFieldNames.Location);

        RuleFor(command => command.CoachId)
            .GreaterThan(0)
            .When(command => command.CoachId.HasValue)
            .WithName(SessionFieldNames.Coach);

        RuleFor(command => command.Capacity)
            .InclusiveBetween(SessionRules.MinimumCapacity, SessionRules.MaximumCapacity)
            .When(command => command.Capacity.HasValue)
            .WithMessage(SessionRules.CapacityMessage);

        RuleFor(command => command.RecurrenceWeeks)
            .InclusiveBetween(SessionRules.MinimumRecurrenceWeeks, SessionRules.MaximumRecurrenceWeeks)
            .WithMessage(SessionRules.RecurrenceMessage);
    }
}
