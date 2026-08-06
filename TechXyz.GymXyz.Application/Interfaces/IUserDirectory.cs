using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// Reads and writes the accounts of the current tenant.
/// <para>
/// The debt this settles is a layering one: accounts live in Persistence
/// because <c>ApplicationUser</c> derives from an Identity type, and Application
/// does not reference Persistence — so <c>IGymDbContext</c> could never expose
/// them. « Équipe &amp; accès » has to read them anyway. This is the seam:
/// declared here, implemented over <c>UserManager</c> on the other side.
/// </para>
/// <para>
/// Every method is scoped to the ambient tenant. Nothing here takes a tenant id,
/// for the same reason no query does: the one place that decides which customer
/// is being served is <c>ITenantContext</c>.
/// </para>
/// </summary>
public interface IUserDirectory
{
    /// <summary>Every account of the tenant, revoked ones included.</summary>
    Task<IReadOnlyList<DirectoryUserDto>> GetTenantUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The accounts behind a set of ids, for the screens that hold people and
    /// want their account beside them. Unknown ids are simply absent from the
    /// result rather than an error: an account can be deleted out from under a
    /// coach record.
    /// </summary>
    Task<IReadOnlyList<DirectoryUserDto>> GetByIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default);

    /// <summary>Null when nobody signs in with that address.</summary>
    Task<DirectoryUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens an account and puts it in a role. Returns null when the address is
    /// already taken — the caller turns that into a refusal the user can read,
    /// rather than the directory guessing at wording.
    /// </summary>
    Task<string?> CreateAccountAsync(
        string email,
        string role,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an account to a single role, dropping the ones it held. False when
    /// the id resolves to nothing in this tenant.
    /// </summary>
    Task<bool> SetRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the door without destroying the account: a revoked coach still
    /// signed the attendance sheets they signed. Implemented with Identity's own
    /// lockout rather than the <c>IsActive</c> convention, which does not apply
    /// here — an account is not an <c>EntityBase</c>.
    /// </summary>
    Task<bool> RevokeAsync(string userId, CancellationToken cancellationToken = default);
}
