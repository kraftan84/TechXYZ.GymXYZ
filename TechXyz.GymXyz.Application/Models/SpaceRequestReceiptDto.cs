namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// What the confirmation screen needs, and nothing more.
/// <para>
/// Handed straight back to the page rather than re-read from a query string:
/// this is a name, an address and a whole application, and putting any of it in a
/// URL would write it into every access log between here and the browser.
/// </para>
/// </summary>
public sealed record SpaceRequestReceiptDto(
    string Reference,
    string ContactFirstName,
    string Name,
    string ContactEmail,
    string RequestedPlan,
    string RequestedSubdomain,
    DateTime ReceivedOn);
