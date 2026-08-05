using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>
/// One attendance sheet, opened. Returns <see cref="SessionRosterDto.Empty"/>
/// when the id resolves to nothing.
/// </summary>
public sealed class GetSessionRosterQuery : IRequest<SessionRosterDto>
{
    public GetSessionRosterQuery(int sessionId)
    {
        SessionId = sessionId;
    }

    public int SessionId { get; }
}
