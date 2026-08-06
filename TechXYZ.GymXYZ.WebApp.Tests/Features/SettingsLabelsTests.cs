using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Components.Features.Reglages;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

public class SettingsLabelsTests
{
    private static readonly DateTime Now = new(2026, 8, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void DayRange_ShouldPrintASingleDayWithoutADash()
    {
        SettingsLabels.DayRange(DayOfWeek.Saturday, DayOfWeek.Saturday).ShouldBe("Samedi");
        SettingsLabels.DayRange(DayOfWeek.Monday, DayOfWeek.Friday).ShouldBe("Lundi – vendredi");
    }

    [Fact]
    public void HoursRange_ShouldPrintOnTheTwentyFourHourClock()
    {
        SettingsLabels.HoursRange(new TimeOnly(6, 30), new TimeOnly(22, 0)).ShouldBe("06:30 – 22:00");
    }

    [Theory]
    [InlineData(0, "à l'instant")]
    [InlineData(30, "il y a 30 min")]
    [InlineData(120, "il y a 2 h")]
    public void LastSeen_ShouldCountInMinutesThenHours(int minutesAgo, string expected)
    {
        SettingsLabels.LastSeen(Now.AddMinutes(-minutesAgo), Now).ShouldBe(expected);
    }

    [Fact]
    public void LastSeen_ShouldSayYesterdayThenCountDays()
    {
        SettingsLabels.LastSeen(Now.AddHours(-30), Now).ShouldBe("hier");
        SettingsLabels.LastSeen(Now.AddDays(-3), Now).ShouldBe("il y a 3 j");
    }

    [Fact]
    public void LastSeen_ShouldFallBackToADate_PastAMonth()
    {
        SettingsLabels.LastSeen(new DateTime(2026, 1, 4, 9, 0, 0, DateTimeKind.Utc), Now)
            .ShouldBe("le 04/01/2026");
    }

    [Fact]
    public void LastSeen_ShouldSayNever_WhenNobodyHasSignedIn()
    {
        SettingsLabels.LastSeen(null, Now).ShouldBe("jamais");
    }

    [Fact]
    public void CurrencyLabel_ShouldPrintTheHandOffWording()
    {
        SettingsLabels.CurrencyLabel("EUR").ShouldBe("Euro (€)");
        SettingsLabels.CurrencyLabel("CHF").ShouldBe("Franc suisse (CHF)");

        // An unknown code prints itself rather than an empty cell.
        SettingsLabels.CurrencyLabel("GBP").ShouldBe("GBP");
    }

    [Fact]
    public void PaymentMethods_ShouldCoverEveryMethodTheModelKnows()
    {
        // The panel is the list of what a gym can be paid in. A method missing
        // here could never be switched on.
        SettingsLabels.PaymentMethods.ShouldBe(Enum.GetValues<PaymentMethod>(), ignoreOrder: true);
    }

    [Fact]
    public void Title_ShouldNameAllSixMessages()
    {
        foreach (var key in Enum.GetValues<NotificationKey>())
        {
            SettingsLabels.Title(key).ShouldNotBeNullOrWhiteSpace();
            SettingsLabels.Description(key).ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void RoleLabel_ShouldWordEveryRoleTheDirectoryCanReturn()
    {
        SettingsLabels.RoleLabel(GymRoleNames.GymManager).ShouldBe("Gestionnaire");
        SettingsLabels.RoleLabel(GymRoleNames.Coach).ShouldBe("Coach");
        SettingsLabels.RoleLabel(GymRoleNames.Member).ShouldBe("Membre");
        SettingsLabels.RoleLabel(GymRoleNames.PlatformAdmin).ShouldBe("Admin TechXYZ");
    }
}
