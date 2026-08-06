using FluentValidation;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateNotificationSettingsCommandValidator
    : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(command => command.Settings).NotEmpty();

        RuleFor(command => command.Settings)
            .Must(settings => settings.Select(setting => setting.Key).Distinct().Count() == settings.Count)
            .WithMessage(SettingsRules.NotificationUnknown);

        RuleForEach(command => command.Settings).ChildRules(setting =>
        {
            setting.RuleFor(input => input.Key).IsInEnum().WithMessage(SettingsRules.NotificationUnknown);

            // A message switched on with no channel would be on and silent —
            // the panel would claim it sends and nothing would leave.
            setting.RuleFor(input => input.Channels)
                .NotEqual(NotificationChannels.None).WithMessage(SettingsRules.ChannelRequired)
                .When(input => input.IsEnabled);
        });
    }
}
