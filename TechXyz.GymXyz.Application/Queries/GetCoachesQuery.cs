using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Coaches grid: free-text search and standing filter, both server-side. No
/// paging — the prototype draws a card grid without a pager.
/// </summary>
public sealed class GetCoachesQuery : IRequest<CoachesPageDto>
{
    /// <summary>Matches first name, last name, role, e-mail or discipline.</summary>
    public string? Search { get; init; }

    /// <summary>Null keeps every standing ("Tous").</summary>
    public CoachStatus? Status { get; init; }
}
