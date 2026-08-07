using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Saves the Identité panel: how the gym presents itself, where it is, and when
/// it is open.
/// <para>
/// The hours travel with the identity rather than in a command of their own
/// because the panel saves as a whole — one « Enregistrer » under all its cards.
/// A separate command would let the address save and the hours fail, leaving the
/// screen showing something nobody chose.
/// </para>
/// <para>
/// The postcode also decides the school-holiday zone, which is cached on the
/// settings row. The handler refreshes it here so the planning banner cannot go
/// on showing the zone of an address the gym has left.
/// </para>
/// </summary>
public sealed class UpdateGymIdentityCommand : IRequest<bool>, IManagerOnly
{
    public UpdateGymIdentityCommand(
        string name,
        string? baseline,
        int? capacity,
        string? siret,
        string? street,
        string? zipCode,
        string? city,
        string? areaLabel,
        string? email,
        string? phone,
        IReadOnlyList<OpeningHoursInput>? openingHours = null)
    {
        Name = name.Trim();
        Baseline = Clean(baseline);
        Capacity = capacity;
        Siret = Clean(siret);
        Street = Clean(street);
        ZipCode = Clean(zipCode);
        City = Clean(city);
        AreaLabel = Clean(areaLabel);
        Email = Clean(email);
        Phone = Clean(phone);
        OpeningHours = openingHours ?? [];
    }

    public string Name { get; }
    public string? Baseline { get; }
    public int? Capacity { get; }
    public string? Siret { get; }
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }

    /// <summary>
    /// Set for a customer who works on the move. When it is, the address is
    /// cleared rather than kept alongside: two ways of saying where somebody is
    /// would let a stale street survive the switch and reappear on a document.
    /// </summary>
    public string? AreaLabel { get; }

    public string? Email { get; }
    public string? Phone { get; }

    public IReadOnlyList<OpeningHoursInput> OpeningHours { get; }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>
/// One line of the hours as the panel submits it. <see cref="Id"/> is 0 for a
/// line the gym has just added.
/// </summary>
public sealed record OpeningHoursInput(
    int Id,
    DayOfWeek DayFrom,
    DayOfWeek DayTo,
    TimeOnly OpensAt,
    TimeOnly ClosesAt);
