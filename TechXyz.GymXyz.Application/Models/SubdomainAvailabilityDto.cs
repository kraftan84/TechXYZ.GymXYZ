namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// What the field says under itself once the server has looked.
/// </summary>
/// <param name="Normalised">The prefix as it would actually be stored.</param>
/// <param name="IsAvailable">True only when it is well formed, free and not reserved.</param>
/// <param name="Message">Ready to show, in the system's voice.</param>
/// <param name="Suggestion">
/// A near miss worth offering when the name is taken, empty otherwise. A
/// suggestion, never a substitution: the applicant chose their own name once and
/// may want a different second choice than the one we would pick.
/// </param>
public sealed record SubdomainAvailabilityDto(
    string Normalised,
    bool IsAvailable,
    string Message,
    string Suggestion = "");
