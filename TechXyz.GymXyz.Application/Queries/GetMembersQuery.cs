using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// Members list: free-text search, standing filter and paging, all server-side.
/// </summary>
public sealed class GetMembersQuery : IRequest<MembersPageDto>
{
    public const int DefaultPageSize = 25;

    /// <summary>Matches first name, last name, e-mail or phone.</summary>
    public string? Search { get; init; }

    /// <summary>Null keeps every standing ("Tous").</summary>
    public MemberStatus? Status { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}
