using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The accounts of the tenant, as the handlers see them. In-memory rather than a
/// real <c>UserManager</c>: what Identity does with a role or a lockout is pinned
/// in <c>UserDirectoryTests</c> on the Persistence side, and repeating it here
/// would test the same thing twice while making these tests need a whole
/// Identity stack to ask one question.
/// </summary>
internal sealed class TestUserDirectory : IUserDirectory
{
    private readonly List<DirectoryUserDto> _users = [];

    public List<string> Revoked { get; } = [];

    public List<(string UserId, string Role)> RoleChanges { get; } = [];

    public List<(string Email, string Role)> Created { get; } = [];

    public TestUserDirectory Add(
        string userId,
        string email,
        string role,
        bool isRevoked = false,
        DateTime? lastSeenAt = null,
        string? displayName = null)
    {
        _users.Add(new DirectoryUserDto(
            userId, email, displayName ?? email, null, role, lastSeenAt, isRevoked));

        return this;
    }

    public Task<IReadOnlyList<DirectoryUserDto>> GetTenantUsersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DirectoryUserDto>>(_users.ToList());

    public Task<IReadOnlyList<DirectoryUserDto>> GetByIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DirectoryUserDto>>(
            _users.Where(user => userIds.Contains(user.UserId)).ToList());

    public Task<DirectoryUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<string?> CreateAccountAsync(
        string email,
        string role,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        Created.Add((email, role));
        var userId = $"user-{_users.Count + 1}";
        Add(userId, email, role, displayName: displayName);

        return Task.FromResult<string?>(userId);
    }

    public Task<bool> SetRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var index = _users.FindIndex(user => user.UserId == userId);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        RoleChanges.Add((userId, role));
        _users[index] = _users[index] with { Role = role };

        return Task.FromResult(true);
    }

    public Task<bool> RevokeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var index = _users.FindIndex(user => user.UserId == userId);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        Revoked.Add(userId);
        _users[index] = _users[index] with { IsRevoked = true };

        return Task.FromResult(true);
    }

    /// <summary>Reset links handed out, so a test can assert one was — or was not.</summary>
    public List<string> ResetsBegun { get; } = [];

    public Task<PasswordResetTicket?> BeginPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(candidate =>
            string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));

        // Same refusal as the real one: a revoked access does not get a way back in.
        if (user is null || user.IsRevoked)
        {
            return Task.FromResult<PasswordResetTicket?>(null);
        }

        ResetsBegun.Add(user.Email);

        return Task.FromResult<PasswordResetTicket?>(
            new PasswordResetTicket(user.Email, $"token-for-{user.UserId}", user.DisplayName));
    }

    public Task<PasswordResetOutcome> CompletePasswordResetAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(candidate =>
            string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));

        if (user is null || user.IsRevoked || token != $"token-for-{user.UserId}")
        {
            return Task.FromResult(PasswordResetOutcome.DeadLink());
        }

        return Task.FromResult(
            newPassword.Length >= 12
                ? PasswordResetOutcome.Ok()
                : PasswordResetOutcome.Refused(["Le mot de passe doit contenir au moins 12 caractères."]));
    }

    public static TestUserDirectory WithManager(string email = "test-user") =>
        new TestUserDirectory().Add("manager", email, GymRoleNames.GymManager);
}
