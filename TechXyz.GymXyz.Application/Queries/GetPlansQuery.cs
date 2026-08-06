using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The formules on sale, in display order. Deliberately parameterless: the
/// abonnements page, the plan picker on a member's record and lot 8's
/// "Formules &amp; tarifs" panel all want the same list, and a query that took a
/// filter would soon be three queries.
/// </summary>
public sealed record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;
