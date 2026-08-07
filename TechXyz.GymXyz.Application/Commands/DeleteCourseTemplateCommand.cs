using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Archives a course template. The hand-off calls this one "Archive"; it is a
/// soft delete like every other, and keeps the repository's naming.
/// </summary>
public sealed class DeleteCourseTemplateCommand : IRequest<bool>, IManagerOnly
{
    public DeleteCourseTemplateCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
