using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The chip on a venue card is derived, never stored — the prototype writes it
/// as free text on every mock entry, and every one of those values follows from
/// something the record already knows.
/// </summary>
public class LocationStatusRulesTests
{
    [Fact]
    public void Resolve_ShouldAnswerByAppointment_ForAHomeVenue()
    {
        LocationStatusRules
            .Resolve(LocationKind.Home, isOpenAccess: false, isWeatherDependent: false)
            .ShouldBe(LocationStatus.ByAppointment);
    }

    [Fact]
    public void Resolve_ShouldAnswerWeatherDependent_ForAnOutdoorVenueThatCaresAboutIt()
    {
        LocationStatusRules
            .Resolve(LocationKind.Outdoor, isOpenAccess: false, isWeatherDependent: true)
            .ShouldBe(LocationStatus.WeatherDependent);
    }

    /// <summary>An outdoor spot the rain does not bother is simply available.</summary>
    [Fact]
    public void Resolve_ShouldAnswerAvailable_ForAnOutdoorVenueWithoutWeatherDependence()
    {
        LocationStatusRules
            .Resolve(LocationKind.Outdoor, isOpenAccess: false, isWeatherDependent: false)
            .ShouldBe(LocationStatus.Available);
    }

    [Fact]
    public void Resolve_ShouldAnswerOpenAccess_ForTheWeightsFloor()
    {
        LocationStatusRules
            .Resolve(LocationKind.Studio, isOpenAccess: true, isWeatherDependent: false)
            .ShouldBe(LocationStatus.OpenAccess);
    }

    [Fact]
    public void Resolve_ShouldAnswerAvailable_ForAPlainStudio()
    {
        LocationStatusRules
            .Resolve(LocationKind.Studio, isOpenAccess: false, isWeatherDependent: false)
            .ShouldBe(LocationStatus.Available);
    }

    /// <summary>
    /// The kind is answered before anything else: a session at the member's
    /// home is "sur rendez-vous" whatever the other flags say.
    /// </summary>
    [Fact]
    public void Resolve_ShouldPreferTheKind_OverTheFlags()
    {
        LocationStatusRules
            .Resolve(LocationKind.Home, isOpenAccess: true, isWeatherDependent: true)
            .ShouldBe(LocationStatus.ByAppointment);
    }
}
