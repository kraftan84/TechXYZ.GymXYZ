using FluentValidation;
using TechXyz.GymXyz.Application.Common;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdatePaymentMethodsCommandValidator : AbstractValidator<UpdatePaymentMethodsCommand>
{
    public UpdatePaymentMethodsCommandValidator()
    {
        RuleFor(command => command.Currency)
            .Matches("^[A-Z]{3}$").WithMessage(SettingsRules.CurrencyInvalid);

        RuleFor(command => command.VatMention)
            .MaximumLength(200);

        // Turning every method off would leave the encaissement drawer with an
        // empty picker and no way to record money that has plainly arrived.
        RuleFor(command => command.AcceptedMethods)
            .NotEmpty().WithMessage(SettingsRules.NoPaymentMethod);
    }
}
