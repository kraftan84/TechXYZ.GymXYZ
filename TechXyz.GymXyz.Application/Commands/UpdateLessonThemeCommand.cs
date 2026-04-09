using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLessonThemeCommand : IRequest<bool>
{
    public UpdateLessonThemeCommand(int id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public int Id { get; }
    public string Name { get; }
    public string? Description { get; }
}
