using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetLocationDetailsPageQuery : IRequest<LocationDetailsPageDto?>
{
    public GetLocationDetailsPageQuery(int locationId)
    {
        LocationId = locationId;
    }

    public int LocationId { get; }
}
