using Bogus;
using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class SessionTests
{
    /// <summary>
    /// A session opens as scheduled without anybody saying so: the planning
    /// creates rows in bulk when a course repeats, and a status left to the
    /// default would otherwise land as the first value of the enum by accident.
    /// </summary>
    [Fact]
    public void Status_ShouldBeScheduled_OnANewSession()
    {
        var session = new Session();

        session.Status.ShouldBe(SessionStatus.Scheduled);
        session.SeriesId.ShouldBeNull();
    }

    /// <summary>
    /// Capacity is copied, never read through the template: changing a course in
    /// the catalogue must leave the sessions already run alone.
    /// </summary>
    [Fact]
    public void Capacity_ShouldNotFollowTheTemplate_OnceCopied()
    {
        var faker = Faker();
        var template = new CourseTemplate(faker.Commerce.ProductName()) { Capacity = 20 };

        var session = new Session { CourseTemplate = template, Capacity = template.Capacity };
        template.Capacity = 12;

        session.Capacity.ShouldBe(20);
    }

    /// <summary>
    /// A slot with nobody animating it is on the planning all the same — the
    /// open-access plateau is the case the prototype draws with a dash.
    /// </summary>
    [Fact]
    public void Coach_ShouldBeOptional()
    {
        var session = new Session { Coach = null, CoachId = null };

        session.Coach.ShouldBeNull();
    }

    private static Faker Faker() => new("en");
}
