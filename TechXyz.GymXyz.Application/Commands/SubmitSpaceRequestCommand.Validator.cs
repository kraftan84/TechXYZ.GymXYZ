using System.Text.RegularExpressions;
using FluentValidation;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// The server's own reading of the form. The six steps refuse the same things as
/// you go, but this is the one that counts: the endpoint is public, so nothing
/// about what reaches it can be assumed to have passed through a screen.
/// </summary>
public sealed partial class SubmitSpaceRequestCommandValidator
    : AbstractValidator<SubmitSpaceRequestCommand>
{
    public SubmitSpaceRequestCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage(command => command.Type == SpaceRequestType.Coach
                ? SpaceRequestRules.SoloNameRequired
                : SpaceRequestRules.NameRequired)
            .MaximumLength(160);

        RuleFor(command => command.ContactFirstName)
            .NotEmpty().WithMessage(SpaceRequestRules.FirstNameRequired)
            .MaximumLength(80);

        RuleFor(command => command.ContactLastName)
            .NotEmpty().WithMessage(SpaceRequestRules.LastNameRequired)
            .MaximumLength(80);

        RuleFor(command => command.ContactEmail)
            .NotEmpty().WithMessage(SpaceRequestRules.EmailRequired)
            .EmailAddress().WithMessage(SpaceRequestRules.EmailInvalid)
            .MaximumLength(160);

        RuleFor(command => command.RequestedSubdomain)
            .NotEmpty().WithMessage(SpaceRequestRules.SubdomainRequired)
            .Must(Subdomains.IsWellFormed).WithMessage(SpaceRequestRules.SubdomainInvalid)
            .Must(candidate => !Subdomains.IsReserved(candidate))
            .WithMessage(SpaceRequestRules.SubdomainReserved)
            .When(command => !string.IsNullOrWhiteSpace(command.RequestedSubdomain));

        RuleFor(command => command.RequestedPlan)
            .Must(PlatformPlans.IsKnown).WithMessage(SpaceRequestRules.PlanUnknown);

        RuleFor(command => command.ZipCode)
            .Must(code => FrenchPostcode().IsMatch(code!))
            .WithMessage(SpaceRequestRules.ZipCodeInvalid)
            .When(command => !string.IsNullOrWhiteSpace(command.ZipCode));

        // Both, and not as a courtesy: the second one is where the applicant is
        // told their data is deleted three months after a refusal. Storing the
        // request without it would be keeping data under a promise nobody made.
        RuleFor(command => command.AcceptedTerms)
            .Equal(true).WithMessage(SpaceRequestRules.ConsentsRequired);

        RuleFor(command => command.AcceptedDataProcessing)
            .Equal(true).WithMessage(SpaceRequestRules.ConsentsRequired);
    }

    [GeneratedRegex(@"^\d{5}$")]
    private static partial Regex FrenchPostcode();
}
