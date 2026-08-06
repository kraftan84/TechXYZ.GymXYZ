using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class NotificationSettingTests
{
    [Fact]
    public void Allows_ShouldRequireBothTheSwitchAndTheChannel()
    {
        var setting = Setting(isEnabled: true, NotificationChannels.Email);

        setting.Allows(NotificationChannels.Email).ShouldBeTrue();
        setting.Allows(NotificationChannels.Sms).ShouldBeFalse();
    }

    [Fact]
    public void Allows_ShouldSendNothing_WhenTheMessageIsSwitchedOff()
    {
        // Channels ticked but the switch down still means "do not send".
        var setting = Setting(isEnabled: false, NotificationChannels.Email | NotificationChannels.Sms);

        setting.Allows(NotificationChannels.Email).ShouldBeFalse();
        setting.Allows(NotificationChannels.Sms).ShouldBeFalse();
    }

    [Fact]
    public void Allows_ShouldSendNothing_WhenTheSettingIsRetired()
    {
        var setting = Setting(isEnabled: true, NotificationChannels.Email);
        setting.IsActive = false;

        setting.Allows(NotificationChannels.Email).ShouldBeFalse();
    }

    [Fact]
    public void Allows_ShouldAnswerFalse_ForTheEmptyChannel()
    {
        // HasFlag alone says every setting carries None, which would make an
        // "is this allowed" check pass for a channel that does not exist.
        var setting = Setting(isEnabled: true, NotificationChannels.Email);

        setting.Allows(NotificationChannels.None).ShouldBeFalse();
    }

    private static NotificationSetting Setting(bool isEnabled, NotificationChannels channels) => new()
    {
        Group = NotificationGroup.MembersAndSubscriptions,
        Key = NotificationKey.RenewalReminder,
        IsEnabled = isEnabled,
        Channels = channels
    };
}
