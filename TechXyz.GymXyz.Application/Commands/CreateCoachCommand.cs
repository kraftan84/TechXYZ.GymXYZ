using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCoachCommand : IRequest<int>, IManagerOnly
{
    public CreateCoachCommand(
        string firstName,
        string lastName,
        string? email,
        string? phone,
        string? street,
        string? zipCode,
        string? city,
        string? country,
        string? roleLabel = null,
        string? bio = null,
        DateOnly? joinedOn = null,
        DateOnly? awayUntil = null,
        IReadOnlyList<bool>? availability = null,
        IReadOnlyList<int>? disciplineIds = null,
        IReadOnlyList<string>? certifications = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Street = street;
        ZipCode = zipCode;
        City = city;
        Country = country;
        RoleLabel = roleLabel;
        Bio = bio;
        JoinedOn = joinedOn;
        AwayUntil = awayUntil;
        Availability = availability;
        DisciplineIds = disciplineIds;
        Certifications = certifications;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Country { get; }
    public string? RoleLabel { get; }
    public string? Bio { get; }

    /// <summary>Defaults to today when the caller does not supply it.</summary>
    public DateOnly? JoinedOn { get; }

    /// <summary>Last day of a leave; null means the coach is around.</summary>
    public DateOnly? AwayUntil { get; }

    /// <summary>Seven flags, Monday to Sunday. Null means "every day".</summary>
    public IReadOnlyList<bool>? Availability { get; }

    /// <summary>Disciplines in display order; the first one carries the brand pill.</summary>
    public IReadOnlyList<int>? DisciplineIds { get; }

    public IReadOnlyList<string>? Certifications { get; }
}
