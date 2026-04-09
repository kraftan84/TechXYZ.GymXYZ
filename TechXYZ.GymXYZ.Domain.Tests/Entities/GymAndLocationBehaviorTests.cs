using Bogus;
using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Domain.Tests;

public class GymAndLocationBehaviorTests
{
    [Fact]
    public void AddLocation_ShouldInitializeCollectionAndAppendLocation()
    {
        var faker = Faker();
        var gym = new Gym(faker.Company.CompanyName());
        var location = new Location(faker.Address.City());

        gym.AddLocation(location);

        gym.Locations.ShouldNotBeNull();
        gym.Locations.Count.ShouldBe(1);
        gym.Locations.First().ShouldBeSameAs(location);
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
    public void AddRoom_ShouldInitializeCollectionAndAppendRoom()
    {
        var faker = Faker();
        var location = new Location(faker.Address.City());
        var room = new Room(faker.Commerce.ProductName());

        location.AddRoom(room);

        location.Rooms.ShouldNotBeNull();
        location.Rooms.Count.ShouldBe(1);
        location.Rooms.First().ShouldBeSameAs(room);
    }

    private static Faker Faker() => new("en");
}
