using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMemberByIdQuery : IRequest<MemberDto?>
{
    public GetMemberByIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
