namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// An account, as everything outside Persistence is allowed to see it. Carries
/// no credential and no Identity type: the point of the directory is that
/// Application can read who signs in without learning what an
/// <c>ApplicationUser</c> is.
/// </summary>
public sealed record DirectoryUserDto(
    string UserId,
    string Email,
    string? DisplayName,
    string? RoleLabel,
    string Role,
    DateTime? LastSeenAt,
    bool IsRevoked);
