using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateMemberCommand : IRequest<int>, IManagerOnly
{
    public CreateMemberCommand(
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

    public string FirstName { get; }
    public string LastName { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Country { get; }

    /// <summary>Defaults to today when the caller does not supply it.</summary>
    public DateOnly? JoinedOn { get; }

    public DateOnly? BirthDate { get; }
    public string? Notes { get; }
}
