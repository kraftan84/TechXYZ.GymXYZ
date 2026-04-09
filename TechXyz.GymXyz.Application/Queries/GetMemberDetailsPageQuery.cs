using MediatR;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetMemberDetailsPageQuery : IRequest<MemberDetailsPageDto?>
{
    public GetMemberDetailsPageQuery(int memberId)
    {
        MemberId = memberId;
    }

    public int MemberId { get; }
}
