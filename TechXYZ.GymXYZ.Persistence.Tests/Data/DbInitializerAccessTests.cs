using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Data;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXYZ.GymXYZ.Persistence.Tests.Data;

/// <summary>
/// Who the seed lets in. Run against the real initializer rather than reading it,
/// because the thing worth pinning is the set of accounts that exists at the end
/// — a login seeded for a role nothing may hold is exactly the shape of the leak
/// this lot closed, and it is invisible in any single line of the seeder.
/// </summary>
public class DbInitializerAccessTests
{
    [Fact]
    public async Task Seed_ShouldOpenNoMemberAccount()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        var members = await fixture.UserManager.GetUsersInRoleAsync(GymRoles.Member);

        members.ShouldBeEmpty("Members are reached by e-mail and never sign in.");
    }

    [Fact]
    public async Task Seed_ShouldLeaveEveryMemberWithoutAnAccountKey()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        var linked = await fixture.DbContext.Members
            .IgnoreQueryFilters()
            .Where(member => member.UserId != null)
            .ToListAsync();

        linked.ShouldBeEmpty();
    }

    [Fact]
    public async Task Seed_ShouldLinkEveryCoachAccountToItsCoach()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        var coachAccounts = await fixture.UserManager.GetUsersInRoleAsync(GymRoles.Coach);
        var linkedUserIds = await fixture.DbContext.Coaches
            .IgnoreQueryFilters()
            .Where(coach => coach.UserId != null)
            .Select(coach => coach.UserId!)
            .ToListAsync();

        // Every coach who signs in is resolvable back to the coach whose sessions
        // they own — the link the "my sessions" filter is about to read.
        coachAccounts.Select(account => account.Id)
            .ShouldBeSubsetOf(linkedUserIds);
        coachAccounts.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Seed_ShouldGiveTheClientBrandItsOwnCoachLogins()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        // A coach account outside GymXYZ, so the partitioning can be proved on a
        // second customer rather than on the brand that happens to have one.
        var marine = await fixture.UserManager.FindByEmailAsync("marine.debord@teamtrainers.fr");
        var najate = await fixture.UserManager.FindByEmailAsync("najate.amzil@teamtrainers.fr");

        marine.ShouldNotBeNull();
        najate.ShouldNotBeNull();
        (await fixture.UserManager.IsInRoleAsync(marine, GymRoles.Coach)).ShouldBeTrue();
        (await fixture.UserManager.IsInRoleAsync(najate, GymRoles.Coach)).ShouldBeTrue();
        marine.TenantId.ShouldBe(najate.TenantId);
    }

    [Fact]
    public async Task Seed_ShouldGiveNajateOneAccountPerHat()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        var salaried = await fixture.UserManager.FindByEmailAsync("najate.amzil@teamtrainers.fr");
        var owner = await fixture.UserManager.FindByEmailAsync("najate.amzil@leyssa-coaching.fr");

        salaried.ShouldNotBeNull();
        owner.ShouldNotBeNull();

        // One account holds one customer and one role, so the same person needs
        // two of them. Pinned rather than tolerated: the day a membership table
        // replaces this, the test says which behaviour is being traded away.
        salaried.TenantId.ShouldNotBe(owner.TenantId);
        (await fixture.UserManager.IsInRoleAsync(salaried, GymRoles.Coach)).ShouldBeTrue();
        (await fixture.UserManager.IsInRoleAsync(owner, GymRoles.GymManager)).ShouldBeTrue();
    }

    [Fact]
    public async Task Seed_ShouldLinkTheSoloManagerToTheCoachSheRuns()
    {
        await using var fixture = await SeedFixture.CreateAsync();

        var owner = await fixture.UserManager.FindByEmailAsync("najate.amzil@leyssa-coaching.fr");
        owner.ShouldNotBeNull();

        var coach = await fixture.DbContext.Coaches
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(candidate => candidate.UserId == owner.Id);

        // Her login address and her coach address differ on purpose, so the link
        // cannot be inferred by matching e-mails.
        coach.ShouldNotBeNull();
        coach.Email.ShouldBe("najate@leyssa-coaching.fr");
    }

    private sealed class SeedFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _services;
        private readonly IServiceScope _scope;

        private SeedFixture(ServiceProvider services, IServiceScope scope)
        {
            _services = services;
            _scope = scope;

            UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            DbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        }

        public UserManager<ApplicationUser> UserManager { get; }

        public GymDbContext DbContext { get; }

        public static async Task<SeedFixture> CreateAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
            services.AddSingleton<ICurrentUserOverride, TestCurrentUserOverride>();
            services.AddScoped<ITenantContext, TestTenantContext>();
            services.AddDbContext<GymDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));

            services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<GymDbContext>();

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();

            await DbInitializer.InitializeAsync(
                scope.ServiceProvider,
                scope.ServiceProvider.GetRequiredService<GymDbContext>());

            return new SeedFixture(provider, scope);
        }

        public async ValueTask DisposeAsync()
        {
            _scope.Dispose();
            await _services.DisposeAsync();
        }
    }

    /// <summary>
    /// A real ambient tenant, unlike the no-op double the directory tests use:
    /// the seeder writes three customers and relies on <c>UseTenant</c> to stamp
    /// each row with the right one.
    /// </summary>
    private sealed class TestTenantContext : ITenantContext
    {
        private readonly Stack<(int TenantId, string? Slug)> _stack = new();

        public int Current => _stack.Count > 0 ? _stack.Peek().TenantId : 0;

        public bool IsResolved => Current != 0;

        public string? Slug => _stack.Count > 0 ? _stack.Peek().Slug : null;

        public IDisposable UseTenant(int tenantId, string? slug = null)
        {
            _stack.Push((tenantId, slug));
            return new Scope(_stack);
        }

        private sealed class Scope(Stack<(int TenantId, string? Slug)> stack) : IDisposable
        {
            public void Dispose() => stack.Pop();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName => "test-user";

        public bool IsInRole(string role) => false;
    }

    private sealed class TestCurrentUserOverride : ICurrentUserOverride
    {
        public string? Current { get; private set; }

        public IDisposable UseTechnicalUser(string userName = "technical")
        {
            Current = userName;
            return new Scope(this);
        }

        private sealed class Scope(TestCurrentUserOverride owner) : IDisposable
        {
            public void Dispose() => owner.Current = null;
        }
    }
}
