using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

/// <summary>Everything the coach record shows, in one round trip.</summary>
public sealed class GetCoachDetailsPageQuery : IRequest<CoachDetailsPageDto?>
{
    public GetCoachDetailsPageQuery(int coachId)
    {
        CoachId = coachId;
    }

    public int CoachId { get; }
}
