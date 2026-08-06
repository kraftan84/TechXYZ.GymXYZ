using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Opens the trail of a platform admin entering a customer's data, and answers
/// with what the caller needs to re-sign the authentication cookie.
/// <para>
/// The row is written <b>before</b> the cookie is re-signed, on purpose: an
/// admin who gets inside without leaving a trace is the failure this entity
/// exists to prevent, so the write is what authorises the entry rather than a
/// consequence of it. A visit recorded for an entry that then fails is the
/// harmless direction of that trade.
/// </para>
/// <para>
/// Returns null when the customer does not exist or is inactive — the caller
/// then re-signs nothing and the admin stays outside.
/// </para>
/// </summary>
public sealed record BeginTenantImpersonationCommand(
    string AdminUserId,
    string AdminEmail,
    int TenantId) : IRequest<TenantImpersonationDto?>;
