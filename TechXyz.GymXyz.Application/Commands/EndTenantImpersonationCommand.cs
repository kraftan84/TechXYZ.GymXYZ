using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Closes the trail of a visit. Takes the admin's own id as well as the visit
/// id so one admin cannot close another's row by posting a number.
/// <para>
/// Answers false when there was nothing open to close — an already-closed visit,
/// or one that belongs to somebody else. The caller signs the admin back out of
/// the customer either way: refusing to leave because the trail is odd would
/// strand them inside.
/// </para>
/// </summary>
public sealed record EndTenantImpersonationCommand(
    string AdminUserId,
    int VisitId) : IRequest<bool>;
