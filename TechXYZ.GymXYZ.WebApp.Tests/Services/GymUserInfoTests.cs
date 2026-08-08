using Shouldly;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXYZ.GymXYZ.WebApp.Tests.Services;

/// <summary>
/// How the Accueil addresses the person in front of it.
/// </summary>
public class GymUserInfoTests
{
    /// <summary>
    /// The nickname is what the account chose to be called, so it wins whenever
    /// there is one — « Bonjour The Rock ».
    /// </summary>
    [Fact]
    public void GreetingName_ShouldPreferTheNickname()
    {
        new GymUserInfo("Dwayne Johnson", "The Rock", "Gérant")
            .GreetingName.ShouldBe("The Rock");
    }

    /// <summary>
    /// Without one, the first name — not the full name. « Bonjour Dwayne
    /// Johnson » reads like a letter from a bank; the topbar is where the whole
    /// name belongs.
    /// </summary>
    [Fact]
    public void GreetingName_ShouldFallBackToTheFirstNameOnly()
    {
        new GymUserInfo("Dwayne Johnson", null, "Gérant")
            .GreetingName.ShouldBe("Dwayne");

        new GymUserInfo("Marine Debord", "   ", "Coach")
            .GreetingName.ShouldBe("Marine");
    }

    /// <summary>
    /// A hyphenated first name is one word and stays whole; a single-word name
    /// is already the answer.
    /// </summary>
    [Theory]
    [InlineData("Jean-Pierre Martin", "Jean-Pierre")]
    [InlineData("Najate", "Najate")]
    [InlineData("  Aurélie  Siquier ", "Aurélie")]
    public void GreetingName_ShouldHandleTheShapesARealNameTakes(string displayName, string expected)
    {
        new GymUserInfo(displayName, null, null).GreetingName.ShouldBe(expected);
    }

    /// <summary>Signed out, the shell still has somebody to greet.</summary>
    [Fact]
    public void GreetingName_ShouldNotBreakForTheAnonymousVisitor()
    {
        GymUserInfo.Anonymous.GreetingName.ShouldBe("Invité");
    }
}
