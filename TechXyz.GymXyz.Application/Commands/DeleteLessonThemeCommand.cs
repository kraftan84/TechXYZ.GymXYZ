using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLessonThemeCommand : IRequest<bool>
{
    public DeleteLessonThemeCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
