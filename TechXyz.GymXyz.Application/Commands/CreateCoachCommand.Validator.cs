using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
{
    public CreateCoachCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName(CoachFieldNames.FirstName);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName(CoachFieldNames.LastName);

        RuleFor(command => command.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email))
            .WithName(CoachFieldNames.Email);

        RuleFor(command => command.Phone)
            .MaximumLength(50)
            .WithName(CoachFieldNames.Phone);

        RuleFor(command => command.Street)
            .MaximumLength(200)
            .WithName(CoachFieldNames.Street);

        RuleFor(command => command.ZipCode)
            .MaximumLength(20)
            .WithName(CoachFieldNames.ZipCode);

        RuleFor(command => command.City)
            .MaximumLength(100)
            .WithName(CoachFieldNames.City);

        RuleFor(command => command.Country)
            .MaximumLength(100)
            .WithName(CoachFieldNames.Country);

        RuleFor(command => command.RoleLabel)
            .MaximumLength(120)
            .WithName(CoachFieldNames.RoleLabel);

        RuleFor(command => command.Bio)
            .MaximumLength(2000)
            .WithName(CoachFieldNames.Bio);

        RuleFor(command => command.JoinedOn)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
            .When(command => command.JoinedOn.HasValue)
            .WithMessage("La date d'arrivée ne peut pas être dans le futur.");

        RuleForEach(command => command.Certifications)
            .MaximumLength(200)
            .WithName(CoachFieldNames.Certifications);

        RuleFor(command => command.Availability!)
            .Must(availability => availability.Count == CoachCompositionHelper.AvailabilityDayCount)
            .When(command => command.Availability is not null)
            .WithMessage("La disponibilité doit couvrir les sept jours de la semaine.");
    }
}
