namespace TechXyz.GymXyz.Application.Models;

public sealed record CoachDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    AddressDto? Address);
