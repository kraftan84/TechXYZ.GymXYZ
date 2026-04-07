using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteMemberCommand : IRequest<bool>
{
    public DeleteMemberCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
