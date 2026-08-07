using MediatR;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The <c>Coach</c> row a signed-in account is, if it is one.
/// <para>
/// Asked once at sign-in, to write the answer into the authentication cookie.
/// The tenant is passed in rather than read from the ambient context: at sign-in
/// the ambient tenant is still whatever the host suggested, which for a coach of
/// another customer on localhost is the wrong gym.
/// </para>
/// </summary>
public sealed record GetCoachIdForAccountQuery(string UserId, int TenantId) : IRequest<int?>;
