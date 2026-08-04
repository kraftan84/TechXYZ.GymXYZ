using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Components.Features.Coachs;

/// <summary>
/// What the create / edit drawer binds to. Dates are <c>DateTime?</c> because
/// that is what the Fluent date picker speaks; the commands take
/// <see cref="DateOnly"/>.
/// </summary>
public sealed class CoachEditModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? RoleLabel { get; set; }
    public string? Bio { get; set; }
    public DateTime? JoinedOn { get; set; }

    /// <summary>Last day of the leave. Empty means the coach is around.</summary>
    public DateTime? AwayUntil { get; set; }

    public string? Street { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    /// <summary>Seven flags, Monday to Sunday.</summary>
    public bool[] Availability { get; init; } = [true, true, true, true, true, false, false];

    /// <summary>
    /// Selected disciplines, in the order they were picked: the first one is
    /// the pill the card tints in the brand colour.
    /// </summary>
    public List<int> DisciplineIds { get; init; } = [];

    public List<string> Certifications { get; init; } = [];

    public DateOnly? JoinedOnOnly => JoinedOn is { } value ? DateOnly.FromDateTime(value) : null;

    public DateOnly? AwayUntilOnly => AwayUntil is { } value ? DateOnly.FromDateTime(value) : null;

    public static CoachEditModel ForCreate() => new()
    {
        JoinedOn = DateTime.Today,
        Country = "France"
    };

    public static CoachEditModel From(CoachDetailsPageDto coach)
    {
        var model = new CoachEditModel
        {
            FirstName = coach.FirstName,
            LastName = coach.LastName,
            Email = coach.Email,
            Phone = coach.Phone,
            RoleLabel = coach.RoleLabel,
            Bio = coach.Bio,
            JoinedOn = coach.JoinedOn.ToDateTime(TimeOnly.MinValue),
            AwayUntil = coach.AwayUntil?.ToDateTime(TimeOnly.MinValue),
            Street = coach.Address?.Street,
            ZipCode = coach.Address?.ZipCode,
            City = coach.Address?.City,
            Country = coach.Address?.Country
        };

        for (var day = 0; day < model.Availability.Length; day++)
        {
            model.Availability[day] = coach.Availability.Days[day];
        }

        // Kept in display order: the drawer edits the rank by reordering.
        model.DisciplineIds.AddRange(coach.Disciplines.Select(discipline => discipline.Id));

        model.Certifications.AddRange(coach.Certifications);

        return model;
    }
}
