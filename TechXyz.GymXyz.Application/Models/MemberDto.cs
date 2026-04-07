namespace TechXyz.GymXyz.Application.Models;

public sealed record MemberDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    bool HasActiveSubscription,
    AddressDto? Address);

public sealed record AddressDto(
    string Street,
    string ZipCode,
    string City,
    string Country);
