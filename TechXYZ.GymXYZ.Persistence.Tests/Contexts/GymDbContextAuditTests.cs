using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Persistence.Tests;

public class GymDbContextAuditTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldStampAuditFields_OnAddedEntity()
    {
        var faker = Faker();
        var currentUser = new TestCurrentUserService { UserName = faker.Internet.UserName() };
        await using var dbContext = CreateDbContext(currentUser);

        var gym = new Gym(faker.Company.CompanyName());
        dbContext.Gyms.Add(gym);

        await dbContext.SaveChangesAsync();

        gym.CreatedBy.ShouldBe(currentUser.UserName);
        gym.UpdatedBy.ShouldBe(currentUser.UserName);
        gym.CreatedOn.ShouldNotBe(default);
        gym.UpdatedOn.ShouldNotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldUpdateUpdatedFields_OnModifiedEntity()
    {
        var faker = Faker();
        var currentUser = new TestCurrentUserService { UserName = faker.Internet.UserName() };
        await using var dbContext = CreateDbContext(currentUser);

        var gym = new Gym(faker.Company.CompanyName());
        dbContext.Gyms.Add(gym);
        await dbContext.SaveChangesAsync();

        var createdBy = gym.CreatedBy;
        var createdOn = gym.CreatedOn;

        currentUser.UserName = faker.Internet.UserName();
        gym.Name = faker.Company.CompanyName();
        await dbContext.SaveChangesAsync();

        gym.CreatedBy.ShouldBe(createdBy);
        gym.CreatedOn.ShouldBe(createdOn);
        gym.UpdatedBy.ShouldBe(currentUser.UserName);
        gym.UpdatedOn.ShouldNotBeNull();
        gym.UpdatedOn.Value.ShouldBeGreaterThanOrEqualTo(createdOn);
    }

    private static GymDbContext CreateDbContext(ICurrentUserService currentUserService)
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GymDbContext(options, currentUserService);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName { get; set; }
    }

    private static Faker Faker() => new("en");
}
