using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteMemberCommand : IRequest<bool>, IManagerOnly
{
    public DeleteMemberCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
