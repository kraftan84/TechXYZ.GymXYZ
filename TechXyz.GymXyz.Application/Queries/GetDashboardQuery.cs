using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Everything the Accueil shows, in one round trip.
/// <para>
/// It takes no parameter. The Accueil is always about the week in progress and
/// the day it is opened on — there is no control on the screen that would move
/// it, and the week strip navigates to the Planning rather than paging in place.
/// </para>
/// </summary>
public sealed class GetDashboardQuery : IRequest<DashboardDto>
{
}
