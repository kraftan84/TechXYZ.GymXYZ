namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// A visit that has just been opened: its trail id, and the customer identity
/// the caller has to write into the authentication cookie.
/// </summary>
public sealed record TenantImpersonationDto(
    int VisitId,
    int TenantId,
    string Slug,
    string DisplayName);
