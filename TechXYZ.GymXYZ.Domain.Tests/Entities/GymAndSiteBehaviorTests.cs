using Bogus;
using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class GymAndSiteBehaviorTests
{
    [Fact]
    public void AddSite_ShouldInitializeCollectionAndAppendSite()
    {
        var faker = Faker();
        var gym = new Gym(faker.Company.CompanyName());
        var site = new Site(faker.Address.City());

        gym.AddSite(site);

        gym.Sites.ShouldNotBeNull();
        gym.Sites.Count.ShouldBe(1);
        gym.Sites.First().ShouldBeSameAs(site);
    }

    [Fact]
    public void AddCoach_ShouldInitializeCollectionAndAppendCoach()
    {
        var faker = Faker();
        var gym = new Gym(faker.Company.CompanyName());
        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());

        gym.AddCoach(coach);

        gym.Coaches.ShouldNotBeNull();
        gym.Coaches.Count.ShouldBe(1);
        gym.Coaches.First().ShouldBeSameAs(coach);
    }

    [Fact]
    public void AddMember_ShouldInitializeCollectionAndAppendMember()
    {
        var faker = Faker();
        var gym = new Gym(faker.Company.CompanyName());
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName());

        gym.AddMember(member);

        gym.Members.ShouldNotBeNull();
        gym.Members.Count.ShouldBe(1);
        gym.Members.First().ShouldBeSameAs(member);
    }

    [Fact]
    public void AddLocation_ShouldInitializeCollectionAndAppendLocation()
    {
        var faker = Faker();
        var site = new Site(faker.Address.City());
        var location = new Location(faker.Commerce.ProductName());

        site.AddLocation(location);

        site.Locations.ShouldNotBeNull();
        site.Locations.Count.ShouldBe(1);
        site.Locations.First().ShouldBeSameAs(location);
    }

    private static Faker Faker() => new("en");
}
