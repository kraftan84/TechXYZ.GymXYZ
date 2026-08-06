using System.Text.RegularExpressions;
using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed partial class UpdateGymIdentityCommandValidator : AbstractValidator<UpdateGymIdentityCommand>
{
    public UpdateGymIdentityCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(SettingsRules.NameRequired)
            .MaximumLength(120);

        RuleFor(command => command.Capacity)
            .InclusiveBetween(1, 100_000).WithMessage(SettingsRules.CapacityOutOfRange)
            .When(command => command.Capacity is not null);

        RuleFor(command => command.Email)
            .EmailAddress().WithMessage(SettingsRules.EmailInvalid)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        // Five digits. Not a general postal-code rule: the school-holiday zone is
        // read off the first two, and that mapping is French.
        RuleFor(command => command.ZipCode)
            .Must(zipCode => FrenchPostcode().IsMatch(zipCode!))
            .WithMessage(SettingsRules.ZipCodeInvalid)
            .When(command => !string.IsNullOrWhiteSpace(command.ZipCode));

        RuleForEach(command => command.OpeningHours).ChildRules(hours =>
        {
            hours.RuleFor(line => line.ClosesAt)
                .Must((line, closesAt) => closesAt > line.OpensAt)
                .WithMessage(SettingsRules.ClosingBeforeOpening);

            // Compared Monday-first: DayOfWeek numbers Sunday zero, so a plain
            // enum comparison would refuse « samedi – dimanche ».
            hours.RuleFor(line => line.DayTo)
                .Must((line, dayTo) => WeekDays.IsForwardRange(line.DayFrom, dayTo))
                .WithMessage(SettingsRules.DayRangeReversed);
        });
    }

    [GeneratedRegex(@"^\d{5}$")]
    private static partial Regex FrenchPostcode();
}
