using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXYZ.GymXYZ.Persistence.Tests.Identity;

/// <summary>
/// The directory is the one seam between Application and Identity, so these
/// exercise it against a real <c>UserManager</c> rather than a stand-in: the
/// behaviour worth pinning — tenant scoping, the lockout, the role swap — all
/// lives in what Identity does, not in what the wrapper says it does.
/// </summary>
public class UserDirectoryTests
{
    private const int TenantId = 1;
    private const int OtherTenantId = 2;

    [Fact]
    public async Task GetTenantUsers_ShouldReturnTheAccountsOfTheCurrentTenantOnly()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();

        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);
        await fixture.AddUserAsync("dwayne@gymxyz.fr", GymRoleNames.GymManager, TenantId);
        await fixture.AddUserAsync("ailleurs@autre.fr", GymRoleNames.GymManager, OtherTenantId);

        var users = await fixture.Directory.GetTenantUsersAsync();

        users.Select(user => user.Email).ShouldBe(["nora@gymxyz.fr", "dwayne@gymxyz.fr"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetTenantUsers_ShouldCarryTheRoleTheAccessScopeIsReadFrom()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var user = (await fixture.Directory.GetTenantUsersAsync()).Single();

        user.Role.ShouldBe(GymRoleNames.Coach);
        TeamAccessScopes.Label(user.Role).ShouldBe(TeamAccessScopes.Coach);
        user.IsRevoked.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIds_ShouldSkipAnIdThatResolvesToNothing()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        var known = await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var users = await fixture.Directory.GetByIdsAsync([known, "un-compte-supprimé"]);

        users.Select(user => user.UserId).ShouldBe([known]);
    }

    [Fact]
    public async Task FindByEmail_ShouldIgnoreCase()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var found = await fixture.Directory.FindByEmailAsync("Nora@GymXYZ.fr");

        found.ShouldNotBeNull();
        found.Email.ShouldBe("nora@gymxyz.fr");
    }

    [Fact]
    public async Task FindByEmail_ShouldNotSeeAnotherCustomerAccount()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("ailleurs@autre.fr", GymRoleNames.GymManager, OtherTenantId);

        var found = await fixture.Directory.FindByEmailAsync("ailleurs@autre.fr");

        found.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAccount_ShouldOpenAPasswordlessAccountInTheRole()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();

        var userId = await fixture.Directory.CreateAccountAsync(
            "theo.garnier@gymxyz.fr", GymRoleNames.Coach, "Théo Garnier");

        userId.ShouldNotBeNull();

        var created = await fixture.Directory.FindByEmailAsync("theo.garnier@gymxyz.fr");
        created.ShouldNotBeNull();
        created.Role.ShouldBe(GymRoleNames.Coach);
        created.DisplayName.ShouldBe("Théo Garnier");

        // Invited, not yet signed up: no password until the invitation is taken up.
        var stored = await fixture.UserManager.FindByIdAsync(userId);
        stored!.PasswordHash.ShouldBeNull();
        stored.TenantId.ShouldBe(TenantId);
    }

    [Fact]
    public async Task CreateAccount_ShouldRefuse_WhenTheAddressIsAlreadyTaken()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var userId = await fixture.Directory.CreateAccountAsync(
            "nora@gymxyz.fr", GymRoleNames.Coach, "Nora Lemoine");

        userId.ShouldBeNull();
    }

    [Fact]
    public async Task SetRole_ShouldReplaceTheRoleRatherThanAddToIt()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        var userId = await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var changed = await fixture.Directory.SetRoleAsync(userId, GymRoleNames.GymManager);

        changed.ShouldBeTrue();

        var user = await fixture.UserManager.FindByIdAsync(userId);
        var roles = await fixture.UserManager.GetRolesAsync(user!);
        roles.ShouldBe([GymRoleNames.GymManager]);
    }

    [Fact]
    public async Task SetRole_ShouldRefuse_ForAnotherCustomerAccount()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        var userId = await fixture.AddUserAsync("ailleurs@autre.fr", GymRoleNames.Coach, OtherTenantId);

        var changed = await fixture.Directory.SetRoleAsync(userId, GymRoleNames.GymManager);

        changed.ShouldBeFalse();
    }

    [Fact]
    public async Task Revoke_ShouldLockTheAccountWithoutDestroyingIt()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        var userId = await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var revoked = await fixture.Directory.RevokeAsync(userId);

        revoked.ShouldBeTrue();

        var user = (await fixture.Directory.GetTenantUsersAsync()).Single();
        user.UserId.ShouldBe(userId);
        user.IsRevoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Revoke_ShouldRefuse_ForAnotherCustomerAccount()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        var userId = await fixture.AddUserAsync("ailleurs@autre.fr", GymRoleNames.Coach, OtherTenantId);

        var revoked = await fixture.Directory.RevokeAsync(userId);

        revoked.ShouldBeFalse();
    }

    // ---- Password reset -----------------------------------------------------
    //
    // The four refusals below all return the same thing: null, or a dead link.
    // That sameness is the security property — the screen shows one sentence
    // whatever the answer, so anything that made these distinguishable would be
    // an account-enumeration oracle no wording could patch over.

    [Fact]
    public async Task BeginPasswordReset_ShouldRefuseAnAddressNobodyUses()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();

        var ticket = await fixture.Directory.BeginPasswordResetAsync("personne@nulle-part.fr");

        ticket.ShouldBeNull();
    }

    [Fact]
    public async Task BeginPasswordReset_ShouldRefuseAnotherCustomersAccount()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("ailleurs@autre.fr", GymRoleNames.GymManager, OtherTenantId);

        var ticket = await fixture.Directory.BeginPasswordResetAsync("ailleurs@autre.fr");

        ticket.ShouldBeNull();
    }

    [Fact]
    public async Task BeginPasswordReset_ShouldRefuseARevokedAccess()
    {
        // The one Identity does not refuse on its own: its reset path never looks
        // at lockout, so a coach whose access was withdrawn could otherwise set a
        // new password and walk straight back in.
        await using var fixture = await DirectoryFixture.CreateAsync();
        var userId = await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);
        await fixture.Directory.RevokeAsync(userId);

        var ticket = await fixture.Directory.BeginPasswordResetAsync("nora@gymxyz.fr");

        ticket.ShouldBeNull();
    }

    [Fact]
    public async Task BeginPasswordReset_ShouldReachThePlatformsOwnAccounts()
    {
        // A platform admin carries no tenant at all, and signs in through this
        // same screen. Scoped to the tenant alone, the reset would silently never
        // find them — and silence is exactly what this flow answers with.
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddPlatformUserAsync("admin@techxyz.fr");

        var ticket = await fixture.Directory.BeginPasswordResetAsync("admin@techxyz.fr");

        ticket.ShouldNotBeNull();
    }

    [Fact]
    public async Task CompletePasswordReset_ShouldChangeThePasswordAndRollTheSecurityStamp()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var before = await fixture.SecurityStampAsync("nora@gymxyz.fr");
        var ticket = await fixture.Directory.BeginPasswordResetAsync("nora@gymxyz.fr");

        var outcome = await fixture.Directory.CompletePasswordResetAsync(
            "nora@gymxyz.fr", ticket!.Token, "Nouveau!Mdp2026");

        outcome.Succeeded.ShouldBeTrue();

        // The stamp moving is what signs the account's other devices out — which
        // the confirmation screen states as a fact, not a hope.
        (await fixture.SecurityStampAsync("nora@gymxyz.fr")).ShouldNotBe(before);
    }

    [Fact]
    public async Task CompletePasswordReset_ShouldCallASpentTokenADeadLink()
    {
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var ticket = await fixture.Directory.BeginPasswordResetAsync("nora@gymxyz.fr");
        await fixture.Directory.CompletePasswordResetAsync("nora@gymxyz.fr", ticket!.Token, "Nouveau!Mdp2026");

        var second = await fixture.Directory.CompletePasswordResetAsync(
            "nora@gymxyz.fr", ticket.Token, "Encore!Autre2026");

        second.Succeeded.ShouldBeFalse();
        second.LinkNoLongerValid.ShouldBeTrue();
        second.PasswordErrors.ShouldBeEmpty();
    }

    [Fact]
    public async Task CompletePasswordReset_ShouldReportARefusedPasswordAsSuchRatherThanAsADeadLink()
    {
        // The two failures land on different screens: one asks for a better
        // password, the other for a new link. Confusing them sends somebody back
        // to their mailbox over a password that was merely too short.
        await using var fixture = await DirectoryFixture.CreateAsync();
        await fixture.AddUserAsync("nora@gymxyz.fr", GymRoleNames.Coach, TenantId);

        var ticket = await fixture.Directory.BeginPasswordResetAsync("nora@gymxyz.fr");

        var outcome = await fixture.Directory.CompletePasswordResetAsync(
            "nora@gymxyz.fr", ticket!.Token, "court1A");

        outcome.Succeeded.ShouldBeFalse();
        outcome.LinkNoLongerValid.ShouldBeFalse();
        outcome.PasswordErrors.ShouldNotBeEmpty();
    }

    [Fact]
    public void IsRevoked_ShouldTellAWithdrawnAccessApartFromFiveBadTries()
    {
        // Same column, two meanings. RevokeAsync writes the far end of time; a
        // failed sign-in writes a quarter of an hour from now.
        var revoked = new ApplicationUser { LockoutEnd = DateTimeOffset.MaxValue };
        var lockedOut = new ApplicationUser { LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15) };
        var open = new ApplicationUser();

        UserDirectory.IsRevoked(revoked).ShouldBeTrue();
        UserDirectory.IsRevoked(lockedOut).ShouldBeFalse();
        UserDirectory.IsRevoked(open).ShouldBeFalse();
    }

    /// <summary>
    /// A real Identity stack over an in-memory store, scoped to
    /// <see cref="TenantId"/> — the tenant the directory believes it serves.
    /// </summary>
    private sealed class DirectoryFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _services;
        private readonly IServiceScope _scope;

        private DirectoryFixture(ServiceProvider services, IServiceScope scope)
        {
            _services = services;
            _scope = scope;

            UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Directory = new UserDirectory(
                scope.ServiceProvider.GetRequiredService<GymDbContext>(),
                UserManager,
                scope.ServiceProvider.GetRequiredService<ITenantContext>());
        }

        public UserManager<ApplicationUser> UserManager { get; }

        public IUserDirectory Directory { get; }

        public static async Task<DirectoryFixture> CreateAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
            services.AddSingleton<ITenantContext>(new TestTenantContext(TenantId));
            services.AddDbContext<GymDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));

            // Data protection and the default token providers are what make a
            // password-reset token exist at all; without them the directory throws
            // instead of answering.
            services.AddDataProtection();

            services.AddIdentityCore<ApplicationUser>(options =>
                {
                    // Mirrors Program.cs. Pinned as a pair by PasswordRuleTests,
                    // which is what stops the screen and the server drifting apart.
                    options.Password.RequiredLength = 12;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<GymDbContext>()
                .AddDefaultTokenProviders();

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            foreach (var role in GymRoles.All)
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }

            return new DirectoryFixture(provider, scope);
        }

        public async Task<string> AddUserAsync(string email, string role, int tenantId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            var created = await UserManager.CreateAsync(user);
            created.Succeeded.ShouldBeTrue();

            await UserManager.AddToRoleAsync(user, role);
            return user.Id;
        }

        /// <summary>An account belonging to no customer — the console's own.</summary>
        public async Task<string> AddPlatformUserAsync(string email)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = null
            };

            (await UserManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
            await UserManager.AddToRoleAsync(user, GymRoleNames.PlatformAdmin);

            return user.Id;
        }

        public async Task<string?> SecurityStampAsync(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);

            return user is null ? null : await UserManager.GetSecurityStampAsync(user);
        }

        public async ValueTask DisposeAsync()
        {
            _scope.Dispose();
            await _services.DisposeAsync();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName => "test-user";

        public bool IsInRole(string role) => false;

        public int? CoachId => null;
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(int tenantId) => Current = tenantId;

        public int Current { get; }

        public bool IsResolved => Current != 0;

        public string? Slug => null;

        public IDisposable UseTenant(int tenantId, string? slug = null) => new NoOpScope();

        private sealed class NoOpScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
