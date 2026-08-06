using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXyz.GymXyz.Persistence.Identity;

/// <summary>
/// <see cref="IUserDirectory"/> over Identity. The only place that turns an
/// <c>ApplicationUser</c> into something the rest of the solution may hold.
/// <para>
/// Reads go through the context rather than <c>UserManager</c>: the panel wants
/// every account with its role in one pass, and asking the manager for each
/// user's roles in turn is a query per row. Writes go through the manager,
/// which owns the normalised columns and the stamps that come with them.
/// </para>
/// </summary>
public sealed class UserDirectory : IUserDirectory
{
    private readonly GymDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;

    public UserDirectory(
        GymDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public Task<IReadOnlyList<DirectoryUserDto>> GetTenantUsersAsync(CancellationToken cancellationToken = default)
        => ProjectAsync(TenantUsers(), cancellationToken);

    public Task<IReadOnlyList<DirectoryUserDto>> GetByIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<DirectoryUserDto>>([]);
        }

        return ProjectAsync(TenantUsers().Where(user => userIds.Contains(user.Id)), cancellationToken);
    }

    public async Task<DirectoryUserDto?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = _userManager.NormalizeEmail(email);

        var matches = await ProjectAsync(
            TenantUsers().Where(user => user.NormalizedEmail == normalized),
            cancellationToken);

        return matches.FirstOrDefault();
    }

    public async Task<string?> CreateAccountAsync(
        string email,
        string role,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (await FindByEmailAsync(email, cancellationToken) is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            TenantId = _tenantContext.Current,
            DisplayName = displayName
        };

        // No password: the account exists so the invitation has something to
        // land on, and the person chooses their own when they take it up.
        // EmailConfirmed stays false until then — accepting is the confirmation.
        var created = await _userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            return null;
        }

        await _userManager.AddToRoleAsync(user, role);
        return user.Id;
    }

    public async Task<bool> SetRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (await FindTenantUserAsync(userId, cancellationToken) is not { } user)
        {
            return false;
        }

        var current = await _userManager.GetRolesAsync(user);

        // Single role per account: the panel offers one, and leaving a stale one
        // behind would keep opening what the change was meant to close.
        var stale = current.Where(held => held != role).ToList();
        if (stale.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, stale);
        }

        if (!current.Contains(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return true;
    }

    public async Task<bool> RevokeAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (await FindTenantUserAsync(userId, cancellationToken) is not { } user)
        {
            return false;
        }

        // Order matters: Identity refuses an end date on an account whose
        // lockout is not enabled, and says so only through a failed result.
        await _userManager.SetLockoutEnabledAsync(user, true);
        var locked = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return locked.Succeeded;
    }

    private IQueryable<ApplicationUser> TenantUsers()
    {
        var tenantId = _tenantContext.Current;
        return _dbContext.Users.Where(user => user.TenantId == tenantId);
    }

    /// <summary>
    /// Tracked, and scoped to the tenant on purpose: the manager would happily
    /// find another customer's account by id.
    /// </summary>
    private Task<ApplicationUser?> FindTenantUserAsync(string userId, CancellationToken cancellationToken)
        => TenantUsers().FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    private async Task<IReadOnlyList<DirectoryUserDto>> ProjectAsync(
        IQueryable<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        var rows = await users
            .AsNoTracking()
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.DisplayName,
                user.RoleLabel,
                user.LastSeenAt,
                user.LockoutEnd,
                Roles = _dbContext.UserRoles
                    .Where(userRole => userRole.UserId == user.Id)
                    .Join(_dbContext.Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        return rows
            .Select(row => new DirectoryUserDto(
                row.Id,
                row.Email ?? row.UserName ?? string.Empty,
                row.DisplayName,
                row.RoleLabel,
                // The widest role held, so somebody who is both decides once.
                row.Roles.OrderBy(RoleRank).FirstOrDefault() ?? GymRoleNames.Member,
                row.LastSeenAt,
                row.LockoutEnd > now))
            .ToList();
    }

    private static int RoleRank(string? role) => role switch
    {
        GymRoleNames.PlatformAdmin => 0,
        GymRoleNames.GymManager => 1,
        GymRoleNames.Coach => 2,
        _ => 3
    };
}
