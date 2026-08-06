using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXYZ.GymXYZ.WebApp.Tests.Features;

public class NotificationOutcomeLabelsTests
{
    private const string Done = "Séance annulée.";

    [Fact]
    public void Warning_ShouldBeSilent_WhenEverythingLeft()
    {
        NotificationOutcomeLabels.Warning(NotificationOutcomeDto.Delivered(3, 0), Done).ShouldBeNull();
    }

    [Fact]
    public void Warning_ShouldSayWhatWasDone_BeforeWhatFailed()
    {
        var warning = NotificationOutcomeLabels.Warning(NotificationOutcomeDto.Delivered(2, 1), Done);

        // The cancellation went through. A message that led with the failure
        // would have somebody cancel the session a second time.
        warning.ShouldNotBeNull();
        warning.ShouldStartWith(Done);
        warning.ShouldContain("1 membre n'a pas pu être prévenu");
    }

    [Fact]
    public void Warning_ShouldPluralise()
    {
        NotificationOutcomeLabels.Warning(NotificationOutcomeDto.Delivered(0, 2), Done)
            .ShouldContain("2 membres n'ont pas pu être prévenus");

        NotificationOutcomeLabels.Warning(NotificationOutcomeDto.Delivered(2, 1), Done)
            .ShouldContain("2 membres prévenus");
    }

    [Fact]
    public void Warning_ShouldExplainASwitchedOffNotification()
    {
        var warning = NotificationOutcomeLabels.Warning(NotificationOutcomeDto.Suppressed, Done);

        warning.ShouldNotBeNull();
        warning.ShouldStartWith(Done);
        warning.ShouldContain("désactivée dans les réglages");
    }

    [Fact]
    public void Warning_ShouldBeSilent_WhenNothingWasSaved()
    {
        // Nothing happened at all — that is a failure, and the failure path
        // reports it. This one only ever describes a success.
        NotificationOutcomeLabels.Warning(NotificationOutcomeDto.NotFound, Done).ShouldBeNull();
    }

    [Fact]
    public void Success_ShouldCountWhatLeft_AndStaySilentWhenNothingHadTo()
    {
        NotificationOutcomeLabels.Success(NotificationOutcomeDto.Delivered(1, 0), Done)
            .ShouldBe("Séance annulée. 1 membre prévenu.");

        NotificationOutcomeLabels.Success(NotificationOutcomeDto.Delivered(3, 0), Done)
            .ShouldBe("Séance annulée. 3 membres prévenus.");

        NotificationOutcomeLabels.Success(NotificationOutcomeDto.SavedOnly, Done).ShouldBe(Done);
    }
}
