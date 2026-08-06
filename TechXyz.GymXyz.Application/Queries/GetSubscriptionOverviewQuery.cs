using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The Abonnements screen in one read. Parameterless like the attendance
/// overview: the filter chips sort rows the page already holds, so switching
/// « Actifs » to « En retard » is not a round trip.
/// </summary>
public sealed record GetSubscriptionOverviewQuery : IRequest<SubscriptionOverviewDto>;
