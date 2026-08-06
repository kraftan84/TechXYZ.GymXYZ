using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Saves the Notifications panel — all six switches at once, as the panel
/// submits them.
/// <para>
/// A row per key is created on the way through for any message the customer has
/// no row for yet: the query reads a missing row as the default rather than as
/// « off », and the first save is what turns that reading into a decision.
/// </para>
/// </summary>
public sealed class UpdateNotificationSettingsCommand : IRequest<bool>
{
    public UpdateNotificationSettingsCommand(IReadOnlyList<NotificationSettingInput> settings)
    {
        Settings = settings;
    }

    public IReadOnlyList<NotificationSettingInput> Settings { get; }
}

public sealed record NotificationSettingInput(
    NotificationKey Key,
    bool IsEnabled,
    NotificationChannels Channels);
