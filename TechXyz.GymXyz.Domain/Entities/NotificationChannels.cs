namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// How a message goes out. Flags, because the hand-off draws several settings
/// carrying both Email and SMS.
/// <para>
/// SMS persists without sending: there is no provider and no budget for one, and
/// the panel says so beside the switch. Storing the preference now means the day
/// a provider is chosen, nobody has to ask three hundred gyms what they wanted.
/// </para>
/// </summary>
[Flags]
public enum NotificationChannels
{
    None = 0,
    Email = 1,
    Sms = 2
}
