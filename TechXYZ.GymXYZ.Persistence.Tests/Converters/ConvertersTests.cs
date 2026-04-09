using Bogus;
using Shouldly;
using TechXyz.GymXyz.Persistence.Converters;

namespace TechXYZ.GymXYZ.Persistence.Tests;

public class ConvertersTests
{
    [Fact]
    public void DateOnlyConverter_ShouldRoundTripDate()
    {
        var faker = Faker();
        var converter = new Converters();
        var date = DateOnly.FromDateTime(faker.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddYears(2)));

        var stored = converter.ConvertToProviderExpression.Compile().Invoke(date);
        var restored = converter.ConvertFromProviderExpression.Compile().Invoke(stored);

        restored.ShouldBe(date);
    }

    [Fact]
    public void TimeOnlyConverter_ShouldRoundTripTime()
    {
        var faker = Faker();
        var converter = new TimeOnlyConverter();
        var time = TimeOnly.FromDateTime(faker.Date.Recent());

        var stored = converter.ConvertToProviderExpression.Compile().Invoke(time);
        var restored = converter.ConvertFromProviderExpression.Compile().Invoke(stored);

        restored.ShouldBe(time);
    }

    private static Faker Faker() => new("en");
}
