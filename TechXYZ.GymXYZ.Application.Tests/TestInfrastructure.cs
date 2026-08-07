using Bogus;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
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
}

/// <summary>
/// The signed-in user, as the tests decide it. Roles are settable because the
/// handlers that reserve an action — reopening a validated attendance sheet —
/// are only testable by handing them somebody who does and does not hold one.
/// </summary>
internal sealed class TestCurrentUserService : ICurrentUserService
{
    public TestCurrentUserService(params string[] roles) => Roles = roles;

    public string? UserName { get; set; } = "test-user";

    public string[] Roles { get; set; }

    /// <summary>
    /// The coach this account is. Null by default, which for a role-less test
    /// user means "restricted to nothing" — so a test that means to be a coach
    /// has to say which one, and cannot pass by accident.
    /// </summary>
    public int? CoachId { get; set; }

    public bool IsInRole(string role) => Roles.Contains(role);

    /// <summary>Somebody who runs the gym: sees and writes everything.</summary>
    public static TestCurrentUserService Manager() => new(GymRoleNames.GymManager);

    /// <summary>A salaried coach, linked to their roster entry.</summary>
    public static TestCurrentUserService Coach(int coachId) =>
        new(GymRoleNames.Coach) { CoachId = coachId };
}
