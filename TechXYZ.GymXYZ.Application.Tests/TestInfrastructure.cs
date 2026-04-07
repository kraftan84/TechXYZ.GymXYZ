using Bogus;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

internal static class TestInfrastructure
{
    public static GymDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new GymDbContext(options, new TestCurrentUserService());
    }

    public static Faker Faker() => new Faker("en");

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName => "test-user";
    }
}
