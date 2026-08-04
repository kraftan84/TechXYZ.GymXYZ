using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.Phone)
            .MaximumLength(50);

        RuleFor(command => command.Street)
            .MaximumLength(200);

        RuleFor(command => command.ZipCode)
            .MaximumLength(20);

        RuleFor(command => command.City)
            .MaximumLength(100);

        RuleFor(command => command.Country)
            .MaximumLength(100);

        RuleFor(command => command.Notes)
            .MaximumLength(2000);

        RuleFor(command => command.BirthDate)
            .LessThan(_ => DateOnly.FromDateTime(DateTime.Today))
            .When(command => command.BirthDate.HasValue)
            .WithMessage("La date de naissance doit être dans le passé.");

        RuleFor(command => command.JoinedOn)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
            .When(command => command.JoinedOn.HasValue)
            .WithMessage("La date d'inscription ne peut pas être dans le futur.");
    }
}
