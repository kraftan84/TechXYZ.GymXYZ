using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// The record's "Dupliquer" button: a new template carrying the same settings
/// and the same coaches. Returns the id of the copy so the page can open it.
/// </summary>
public sealed class DuplicateCourseTemplateCommand : IRequest<int?>, IManagerOnly
{
    public DuplicateCourseTemplateCommand(int id)
    {
        Id = id;
    }

    public int Id { get; }
}
