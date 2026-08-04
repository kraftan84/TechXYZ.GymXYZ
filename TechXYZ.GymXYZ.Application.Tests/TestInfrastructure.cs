using Bogus;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

internal static class TestInfrastructure
{
    /// <summary>Tenant every fixture writes to, unless a test asks for another one.</summary>
    public const int DefaultTenantId = 1;

    public static GymDbContext CreateDbContext(string databaseName)
        => CreateDbContext(databaseName, new TestTenantContext(DefaultTenantId));

    public static GymDbContext CreateDbContext(string databaseName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new GymDbContext(options, new TestCurrentUserService(), tenantContext);
    }

    public static Faker Faker() => new Faker("en");

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName => "test-user";
    }
}
