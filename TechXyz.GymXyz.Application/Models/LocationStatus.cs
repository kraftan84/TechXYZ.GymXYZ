namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The chip on a venue card. Derived from the venue's own kind and flags, never
/// stored — the prototype writes these as free text on each mock entry, but
/// every one of them follows from something the record already knows.
/// <para>
/// One of the prototype's five values is still missing on purpose: "Beau temps"
/// reads a live forecast, which ships with the weather lot after lot 8. It is
/// not guessed meanwhile.
/// </para>
/// </summary>
public enum LocationStatus
{
    /// <summary>"Disponible" — a studio that takes bookings.</summary>
    Available,

    /// <summary>"Forte demande" — the venue's sessions come back nearly full.</summary>
    HighDemand,

    /// <summary>"Accès libre" — open during opening hours, nothing to book.</summary>
    OpenAccess,

    /// <summary>"Météo-dépendant" — an outdoor spot the rain can call off.</summary>
    WeatherDependent,

    /// <summary>"Sur rendez-vous" — a session at the member's home.</summary>
    ByAppointment
}
