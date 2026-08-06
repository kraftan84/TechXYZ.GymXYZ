using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// The way out of the building. Declared here and implemented on the
/// infrastructure side, like <see cref="ISchoolCalendarService"/>: the handlers
/// decide <em>whether</em> a message goes and what it says, never how it
/// travels.
/// <para>
/// The contract is that it never throws. Every failure comes back as a
/// described <see cref="EmailDeliveryResult"/>, because by the time a handler
/// calls this it has already committed something — a stamped relance, a
/// cancelled session — and an exception would roll back work the user asked for
/// on the strength of somebody else's outage.
/// </para>
/// </summary>
public interface IEmailSender
{
    Task<EmailDeliveryResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
