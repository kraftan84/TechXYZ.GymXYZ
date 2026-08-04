using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateCoachCommand : IRequest<bool>
{
    public UpdateCoachCommand(
        int id,
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
        Id = id;
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

    public int Id { get; }
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

    /// <summary>Left untouched when null, so a partial edit never resets it.</summary>
    public DateOnly? JoinedOn { get; }

    /// <summary>
    /// The end of the current leave. Unlike the other optional fields this one
    /// is applied as given: passing null is how a leave is cancelled, and the
    /// drawer always sends the value it shows.
    /// </summary>
    public DateOnly? AwayUntil { get; }

    /// <summary>Seven flags, Monday to Sunday. Left untouched when null.</summary>
    public IReadOnlyList<bool>? Availability { get; }

    /// <summary>Replaces the whole set when given; left untouched when null.</summary>
    public IReadOnlyList<int>? DisciplineIds { get; }

    /// <summary>Replaces the whole list when given; left untouched when null.</summary>
    public IReadOnlyList<string>? Certifications { get; }
}
