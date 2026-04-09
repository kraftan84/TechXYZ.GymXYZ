using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLessonCommand : IRequest<bool>
{
    public DeleteLessonCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
