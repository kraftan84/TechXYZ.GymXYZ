using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// The week to put on the poster. <paramref name="WeekStart"/> is normalised to
/// the Monday of the week it falls in, like the planning grid, so the caller can
/// hand over whichever day its screen is showing.
/// <para>
/// <see cref="IManagerOnly"/> is the point of having a query of its own rather
/// than reusing <see cref="GetWeekPlanningQuery"/>. That one applies
/// <c>CoachScope</c> as a floor, so a coach asking for the week receives
/// <b>their</b> sessions — correct for a screen, and a lie on an image titled
/// « Planning de la semaine ». The perimeter of the content is not the perimeter
/// of the audience, so this refuses rather than narrows.
/// </para>
/// <para>
/// The toolbar's coach / venue / format chips are deliberately absent: the
/// poster always carries the whole week. Publishing a week filtered down to one
/// coach without saying so is the same lie by another route, and saying so would
/// need a headline nobody has designed.
/// </para>
/// </summary>
public sealed record GetPlanningPosterQuery(DateOnly WeekStart)
    : IRequest<PosterWeekDto>, IManagerOnly;
