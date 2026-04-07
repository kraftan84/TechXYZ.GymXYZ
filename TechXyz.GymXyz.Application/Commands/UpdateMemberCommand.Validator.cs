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
    }
}
