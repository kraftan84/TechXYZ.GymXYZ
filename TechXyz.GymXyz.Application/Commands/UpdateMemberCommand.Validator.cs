using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithName(MemberFieldNames.Id);

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName(MemberFieldNames.FirstName);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName(MemberFieldNames.LastName);

        RuleFor(command => command.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email))
            .WithName(MemberFieldNames.Email);

        RuleFor(command => command.Phone)
            .MaximumLength(50)
            .WithName(MemberFieldNames.Phone);

        RuleFor(command => command.Street)
            .MaximumLength(200)
            .WithName(MemberFieldNames.Street);

        RuleFor(command => command.ZipCode)
            .MaximumLength(20)
            .WithName(MemberFieldNames.ZipCode);

        RuleFor(command => command.City)
            .MaximumLength(100)
            .WithName(MemberFieldNames.City);

        RuleFor(command => command.Country)
            .MaximumLength(100)
            .WithName(MemberFieldNames.Country);

        RuleFor(command => command.Notes)
            .MaximumLength(2000)
            .WithName(MemberFieldNames.Notes);

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
