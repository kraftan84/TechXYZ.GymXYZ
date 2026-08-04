using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommand : IRequest<bool>
{
    public UpdateMemberCommand(
        int id,
        string firstName,
        string lastName,
        string? email,
        string? phone,
        string? street,
        string? zipCode,
        string? city,
        string? country,
        DateOnly? joinedOn = null,
        DateOnly? birthDate = null,
        string? notes = null)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Street = street;
        ZipCode = zipCode;
        City = city;
        Country = country;
        JoinedOn = joinedOn;
        BirthDate = birthDate;
        Notes = notes;
    }

    public int Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Country { get; }

    /// <summary>Left untouched when null, so a partial edit never resets it.</summary>
    public DateOnly? JoinedOn { get; }

    public DateOnly? BirthDate { get; }
    public string? Notes { get; }
}
